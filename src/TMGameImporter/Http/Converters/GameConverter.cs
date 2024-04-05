using Microsoft.Extensions.Logging;
using TMGameImporter.Http.Converters.Models;
using TMModels;
using TMModels.ThroneMaster;

namespace TMGameImporter.Http.Converters;

internal class GameConverter
{
    private readonly LogConverter _logConverter;
    private readonly StateConverter _stateConverter;
    private readonly ILogger<GameConverter> _logger;

    public GameConverter(LogConverter logConverter, StateConverter stateConverter, ILogger<GameConverter> logger)
    {
        _logConverter = logConverter;
        _stateConverter = stateConverter;
        _logger = logger;
    }

    public Game? Convert(int gameId, StateRaw stateRaw, StateRaw chatRaw, string logHtmlString)
    {
        var state = _stateConverter.Convert(stateRaw, chatRaw);
        if (state?.GameId != gameId)
        {
            _logger.LogError("Fetched game ID {fetchedGameId} is different than expected: {expectedGameId}.", state?.GameId, gameId);
            return null;
        }

        var isStalling = state.Chat?.Contains("delay!", StringComparison.InvariantCultureIgnoreCase) ?? false;

        var log = _logConverter.Convert(gameId, logHtmlString);

        var houses = GetHouses(state, log);

        return new Game(gameId, state.Name, state.IsFinished, isStalling, state.Turn,
            state.Map, GetWesteros(log?.Logs, state.Turn), GetRavenActions(log?.Logs, state.Turn),
            houses, DateTimeOffset.UtcNow);
    }

    private HouseScore[] GetHouses(State state, Log? log)
    {
        const int houseSize = 6;

        var logsPerTurn = log?.Logs.GroupBy(item => item.Turn).ToArray();

        var houses = state.HousesOrder.Select((house, i) =>
                GetHouseScore(i, house, state, state.HousesDataRaw[(i * houseSize)..((i + 1) * houseSize)], logsPerTurn))
            .ToArray();

        Array.Sort(houses);
        Array.Reverse(houses);

        if (log == null)
            return houses;

        try
        {
            houses = houses.CalculateStats(log);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Calculating stats for game {gameId} failed.", state.GameId);
        }
        return houses;
    }

    private static HouseScore GetHouseScore(int idx, House house, State state, string houseDataRaw, IGrouping<int, LogItem>[]? logsPerTurn)
    {
        var player = state.Players[idx];
        var throne = int.Parse(houseDataRaw[..1]);
        var fiefdoms = int.Parse(houseDataRaw[1..2]);
        var kingsCourt = int.Parse(houseDataRaw[2..3]);
        var supplies = int.Parse(houseDataRaw[3..4]);
        var powerTokens = int.Parse(houseDataRaw[4..6]);
        var strongholds = 0;
        var castles = 0;
        var cla = 0;
        foreach (var land in state.Map.Lands)
        {
            if (land.House != house)
                continue;

            cla++;
            if (land.MobilizationPoints == 1)
                castles++;
            else if (land.MobilizationPoints == 2)
                strongholds++;
        }

        var houseSpeed = state.Stats.FirstOrDefault(speed => speed.House == house);

        var battlesInTurn = logsPerTurn?
            .Select(items => items
                .Count(item => IsPlayerBattleLogItem(house, item)))
            .ToArray() ?? Array.Empty<int>();

        var turn = logsPerTurn?.Select(items =>
            items.Any(log =>
                log.House == house && log.Phase == Phase.Planning)
                ? items.Key
                : 0)
            .Max() ?? 0;

        return new HouseScore(house, player, throne,
            fiefdoms, kingsCourt, supplies, powerTokens, strongholds,
            castles, cla, houseSpeed?.MinutesPerMove ?? 0, houseSpeed?.MovesCount ?? 0, battlesInTurn, turn, new Stats());
    }

    private static bool IsPlayerBattleLogItem(House house, LogItem item) =>
        item.Phase == Phase.March &&
        item.Message.Contains("Battle!") &&
        item.Message.Contains(house.ToString(), StringComparison.InvariantCultureIgnoreCase);

    private static WesterosStats? GetWesteros(IReadOnlyCollection<LogItem>? logs, int turn)
    {
        if (logs == null)
            return null;

        var westerosStats = new WesterosStats(
            GetArrayWithEmptyArrays<WesterosPhase1>(turn),
            GetArrayWithEmptyArrays<WesterosPhase2>(turn),
            GetArrayWithEmptyArrays<WesterosPhase3>(turn),
            GetArrayWithEmptyArrays<Wildling>(turn),
            turn);
        IWesterosConverter phaseConverter = new WesterosPhase1Converter();

        foreach (var log in logs.Where(log => log.Phase == Phase.Westeros))
            phaseConverter = phaseConverter.Parse(log, westerosStats);

        return westerosStats;
    }

    private static RavenAction[]? GetRavenActions(IReadOnlyCollection<LogItem>? logs, int turn)
    {
        if (logs == null)
            return null;

        var actions = new RavenAction[turn];
        foreach (var log in logs)
        {
            if (log.Phase == Phase.Raven)
                actions[log.Turn - 1] = new RavenAction(GetRavenActionType(log.Message), log.House);
            else if (log.Message.Contains("did not use the messenger raven."))
                actions[log.Turn - 1] = new RavenAction(RavenActionType.Nothing, log.House);
            else if (log.Message.Contains("placed the card at the bottom of the Wildling deck."))
                actions[log.Turn - 1] = new RavenAction(RavenActionType.Discarded, log.House);
            else if (log.Message.Contains("knows things."))
                actions[log.Turn - 1] = new RavenAction(RavenActionType.Knows, log.House);
        }

        return actions;
    }

    private static RavenActionType GetRavenActionType(string logMessage)
    {
        if (logMessage.Contains("decided to look in the Wildling deck."))
            return RavenActionType.Look;
        if (logMessage.Contains(" replaced ") && logMessage.Contains("Order"))
            return RavenActionType.Replaced;
        return RavenActionType.Nothing;
    }

    private static T[][] GetArrayWithEmptyArrays<T>(int turn) =>
        Enumerable.Range(0, turn).Select(_ => Array.Empty<T>()).ToArray();
}