using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;
using System.Web;
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

    public State? Convert(StateRaw stateRaw, StateRaw? chatRaw)
    {
        try
        {
            var chat = GetChat(chatRaw?.Chat);
            var data = new Data(stateRaw.Data);
            var setup = new Setup(stateRaw.Setup);
            var stats = stateRaw.Stats
                .Select(row => row.Split(','))
                .Where(row => row.Length == 4)
                .Select(row => new HouseSpeed(Houses.Parse(row[0]),
                    double.Parse(row[2], CultureInfo.InvariantCulture), int.Parse(row[3])))
                .ToArray();

            return new State(
                GetGameId(setup),
                GetName(setup),
                stateRaw.Time,
                GetIsFinished(setup),
                GetTurn(data),
                GetHousesDataRaw(data),
                GetHousesOrder(setup),
                GetPlayers(setup),
                GetMap(setup, data),
                stats,
                chat,
                GetLastAction(data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while converting State data.");
            return null;
        }
    }

    private static string? GetChat(object[]? chat)
    {
        if (chat == null)
            return null;

        var result = chat.OfType<JsonElement>()
                   .Where(item => item.ValueKind == JsonValueKind.String)
                   .Select(item => item.GetString())
                   .FirstOrDefault(item => item?.Contains("The battle for Westeros begins, now!") ?? false);

        if (result != null)
            return result;

        if (chat.Length < 3)
            return null;

        return ((JsonElement?)chat[3])?.GetString();
    }

    private static int? GetGameId(Setup setup) => int.Parse(setup["g-id"]);

    private static string GetName(Setup setup) => setup["g-ttl"];

    private static bool GetIsFinished(Setup setup) => setup["g-stts"] != "1";

    private static int GetTurn(Data data) => int.Parse(data["gamestate"][..1]) + 1;

    private static string GetHousesDataRaw(Data data) => data["houses"];

    private static House[] GetHousesOrder(Setup setup) => setup["g-hid"]
        .Split('|')
        .Select(Houses.Parse)
        .ToArray();

    private static string[] GetPlayers(Setup setup) => setup["g-hid"]
        .Split('|')
        .Select(s => HttpUtility.UrlDecode(setup[$"p-h{s.ToLower()}"]))
        .ToArray();

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

        var id = int.Parse(areaId[1..]);

        return new Area(isEnabled, type, id, areaRaw[3].GetString()!);
    }

    private static List<Land> GetLands(string landsRaw, IReadOnlyDictionary<int, Area> areas)
    {
        const int landSize = 8;

        var lands = new List<Land>();
        for (var i = 0; i < areas.Count; i++)
        {
            var landRaw = landsRaw[(i * landSize)..((i + 1) * landSize)];
            var area = areas[i];
            var house = Houses.Parse(landRaw[0]);
            var footmen = int.Parse(landRaw[1..2]);
            var knights = int.Parse(landRaw[2..3]);
            var siegeEngines = int.Parse(landRaw[3..4]);
            var powerTokens = int.Parse(landRaw[4..5]);
            var mobilizationPoints = int.Parse(landRaw[5..6]);
            var supplies = int.Parse(landRaw[6..7]);
            var crowns = int.Parse(landRaw[7..8]);
            lands.Add(new Land(area.IsEnabled, area.Id,
                area.Name, house, footmen, knights, siegeEngines,
                powerTokens, mobilizationPoints, supplies, crowns));
        }

        return lands;
    }

    private static List<Sea> GetSeas(string seasRaw, IReadOnlyDictionary<int, Area> areas)
    {
        const int seaSize = 2;

        var seas = new List<Sea>();
        for (var i = 0; i < areas.Count; i++)
        {
            var seaRaw = seasRaw[(i * seaSize)..((i + 1) * seaSize)];
            var area = areas[i];
            var house = Houses.Parse(seaRaw[0]);
            var ships = int.Parse(seaRaw[1..2]);
            seas.Add(new Sea(area.IsEnabled, area.Id,
                area.Name, house, ships));
        }

        return seas;
    }

    private static List<Port> GetPorts(string portsRaw, IReadOnlyDictionary<int, Area> areas)
    {
        var ports = new List<Port>();
        for (var i = 0; i < areas.Count; i++)
        {
            var area = areas[i];
            var ships = int.Parse(portsRaw[..1]);
            ports.Add(new Port(area.IsEnabled, area.Id,
                area.Name, ships));
        }

        return ports;
    }

    private DateTimeOffset? GetLastAction(Data data) =>
        double.TryParse(data["lastaction"], out var lastActionTime) ? DateTimeOffset.FromUnixTimeSeconds((long)lastActionTime) : null;
}