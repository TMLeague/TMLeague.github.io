using Microsoft.Extensions.Logging;
using System.Globalization;
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

    public static State Convert(StateRaw stateRaw, StateRaw? chatRaw)
    {
        var chat = chatRaw?.Chat.FirstOrDefault(item =>
            item is string str && str.StartsWith("The battle for Westeros begins, now!")) as string;
        var data = stateRaw.Data
            .Select(row => row.Split(','))
            .Where(row => row.Length == 3)
            .ToDictionary(row => row[1], row => GetValue(row[0], row[2]));
        var setup = stateRaw.Setup
            .Select(row => row.Split(','))
            .Where(row => row.Length == 3)
            .ToDictionary(row => row[1], row => GetValue(row[0], row[2]));
        var stats = stateRaw.Stats
            .Select(row => row.Split(','))
            .Where(row => row.Length == 4)
            .Select(row => new HouseSpeed(HouseParser.Parse(row[0]), double.Parse(row[2]), uint.Parse(row[3])))
            .ToArray();

        var gameId = (uint?)(data["g-id"] as int?);
        var isFinished = setup["g-stts"] is int value && value != 1;
        var turn = GetTurn(data["gamestate"] as string);
        var mapDefinition = GetMapDefinition(setup["g-mpd"] as string);
        return new State(stateRaw.Time, chat, gameId, isFinished, turn, mapDefinition, stats);
    }

    private static object GetValue(string isNumber, string rowValue)
    {
        return isNumber == "1" ?
            rowValue.Length > 1 ?
                int.Parse(rowValue) :
                int.Parse(rowValue, NumberStyles.HexNumber) :
            rowValue;
    }

    private static uint GetTurn(string? gameState)
    {
        if (gameState?.Length >= 1 &&
            uint.TryParse(gameState[0].ToString(), out var zeroBasedTurn))
            return zeroBasedTurn + 1;
        return 0;
    }

    private static string[][] GetMapDefinition(string? s)
    {
        throw new NotImplementedException();
    }
}