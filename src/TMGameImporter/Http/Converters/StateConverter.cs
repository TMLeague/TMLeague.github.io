﻿using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;
using TMGameImporter.Http.Converters.Models;
using TMModels;
using TMModels.ThroneMaster;

namespace TMGameImporter.Http.Converters;

internal class StateConverter
{
    private readonly ILogger<StateConverter> _logger;

    public StateConverter(ILogger<StateConverter> logger)
    {
        _logger = logger;
    }

    public State Convert(StateRaw stateRaw, StateRaw? chatRaw)
    {
        var chat = chatRaw?.Chat.FirstOrDefault(item =>
            item is string str && str.StartsWith("The battle for Westeros begins, now!")) as string;
        var data = new Data(stateRaw.Data);
        var setup = new Setup(stateRaw.Setup);
        var stats = stateRaw.Stats
            .Select(row => row.Split(','))
            .Where(row => row.Length == 4)
            .Select(row => new HouseSpeed(HouseParser.Parse(row[0]), double.Parse(row[2], CultureInfo.InvariantCulture), uint.Parse(row[3])))
            .ToArray();

        return new State(
            stateRaw.Time,
            chat,
            GetGameId(setup),
            GetIsFinished(setup),
            GetTurn(data),
            GetMap(setup, data),
            stats);
    }

    private static uint? GetGameId(Setup setup) =>
        setup.TryGetValue("g-id", out var gId) ? uint.Parse(gId) : null;

    private static bool GetIsFinished(Setup setup) =>
        setup.TryGetValue("g-stts", out var gStts) && gStts != "1";

    private static uint GetTurn(Data data)
    {
        if (!data.TryGetValue("gamestate", out var gameState))
            return 0;

        if (gameState?.Length >= 1 &&
            uint.TryParse(gameState[..1], out var zeroBasedTurn))
            return zeroBasedTurn + 1;
        return 0;
    }

    private Map GetMap(Setup setup, Data data)
    {
        try
        {
            var gMpd = setup["g-mpd"];
            var landsRaw = data["land"];
            var seasRaw = data["sea"];
            var portsRaw = data["port"];

            var gMpdJson = gMpd.Replace(';', ',');
            var gMpdArray = JsonSerializer.Deserialize<JsonElement[][]>(gMpdJson)!;

            var areas = gMpdArray
                .Select(GetArea)
                .GroupBy(area => area.Type)
                .ToDictionary(
                    grouping => grouping.Key,
                    grouping => grouping
                        .ToDictionary(
                            area => area.Id,
                            area => area));

            var lands = GetLands(landsRaw, areas[AreaType.Land]);
            var seas = GetSeas(seasRaw, areas[AreaType.Sea]);
            var ports = GetPorts(portsRaw, areas[AreaType.Port]);

            return new Map(lands.ToArray(), seas.ToArray(), ports.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while converting Map data.");
            return new Map(Array.Empty<Land>(), Array.Empty<Sea>(), Array.Empty<Port>());
        }
    }

    private static Area GetArea(IReadOnlyList<JsonElement> areaRaw)
    {
        var isEnabled = areaRaw[0].GetInt16() == 1;

        var areaId = areaRaw[2].GetString()!;
        var type = areaId[0] switch
        {
            'l' => AreaType.Land,
            's' => AreaType.Sea,
            'p' => AreaType.Port,
            _ => AreaType.Unknown
        };

        var id = ushort.Parse(areaId[1..]);

        return new Area(isEnabled, type, id, areaRaw[3].GetString()!);
    }

    private static List<Land> GetLands(string landsRaw, IReadOnlyDictionary<ushort, Area> areas)
    {
        const int landSize = 8;

        var lands = new List<Land>();
        for (ushort i = 0; i < areas.Count; i++)
        {
            var landRaw = landsRaw[(i * landSize)..((i + 1) * landSize)];
            var area = areas[i];
            var house = HouseParser.Parse(landRaw[0]);
            var footmen = ushort.Parse(landRaw[1..2]);
            var knights = ushort.Parse(landRaw[2..3]);
            var siegeEngines = ushort.Parse(landRaw[3..4]);
            var powerTokens = ushort.Parse(landRaw[4..5]);
            var mobilizationPoints = ushort.Parse(landRaw[5..6]);
            var supplies = ushort.Parse(landRaw[6..7]);
            var crowns = ushort.Parse(landRaw[7..8]);
            lands.Add(new Land(area.IsEnabled, area.Id,
                area.Name, house, footmen, knights, siegeEngines,
                powerTokens, mobilizationPoints, supplies, crowns));
        }

        return lands;
    }

    private static List<Sea> GetSeas(string seasRaw, IReadOnlyDictionary<ushort, Area> areas)
    {
        const int seaSize = 2;

        var seas = new List<Sea>();
        for (ushort i = 0; i < areas.Count; i++)
        {
            var seaRaw = seasRaw[(i * seaSize)..((i + 1) * seaSize)];
            var area = areas[i];
            var house = HouseParser.Parse(seaRaw[0]);
            var ships = ushort.Parse(seaRaw[1..2]);
            seas.Add(new Sea(area.IsEnabled, area.Id,
                area.Name, house, ships));
        }

        return seas;
    }

    private static List<Port> GetPorts(string portsRaw, IReadOnlyDictionary<ushort, Area> areas)
    {
        var ports = new List<Port>();
        for (ushort i = 0; i < areas.Count; i++)
        {
            var area = areas[i];
            var ships = ushort.Parse(portsRaw[..1]);
            ports.Add(new Port(area.IsEnabled, area.Id,
                area.Name, ships));
        }

        return ports;
    }
}