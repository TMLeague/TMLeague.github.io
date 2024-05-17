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

        var westerosStats = GetWesteros(log?.Logs, state.Turn);

        var ravenActions = GetRavenActions(log?.Logs, state.Turn);

        var wildlingKnowledge = GetWildlingKnowledge(state.HousesOrder, westerosStats?.Wildlings, ravenActions);

        var houses = GetHouses(state, log, wildlingKnowledge);

        return new Game(gameId, state.Name, state.IsFinished, isStalling, state.Turn,
            state.Map, westerosStats, ravenActions, houses, DateTimeOffset.UtcNow, log?.IsProfessional ?? true);
    }

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

    private static RavenAction?[]? GetRavenActions(IReadOnlyCollection<LogItem>? logs, int turn)
    {
        if (logs == null)
            return null;

        var actions = new RavenAction?[turn];
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

    private WildligKnowledge? GetWildlingKnowledge(House[] houses, Wildling[][]? wildlingsByTurn, RavenAction?[]? ravenActions)
    {
        if (wildlingsByTurn == null || ravenActions == null)
            return null;

        var knowledge = new WildligKnowledge(houses.ToDictionary(house => house, _ => new HouseWildligKnowledge(false, 0)));

        foreach (var (wildlings, ravenAction) in wildlingsByTurn.Zip(ravenActions))
        {
            foreach (var _ in wildlings)
                foreach (var (house, houseKnowledge) in knowledge.ToArray())
                    knowledge[house] = houseKnowledge.Attack();

            if (ravenAction?.Type is RavenActionType.Discarded or RavenActionType.Knows or RavenActionType.Look)
                knowledge[ravenAction.House] = knowledge[ravenAction.House].Know();

            if (ravenAction?.Type == RavenActionType.Discarded)
                foreach (var (house, houseKnowledge) in knowledge.Where(pair => pair.Value.Knows).ToArray())
                    knowledge[house] = houseKnowledge.Discarded();
        }

        return knowledge;
    }

    private HouseScore[] GetHouses(State state, Log? log, WildligKnowledge? wildligKnowledge)
    {
        const int houseSize = 6;

        var logsPerTurn = log?.Logs.GroupBy(item => item.Turn).ToArray();

        var houses = state.HousesOrder.Select((house, i) =>
                GetHouseScore(i, house, state, state.HousesDataRaw[(i * houseSize)..((i + 1) * houseSize)], logsPerTurn, wildligKnowledge))
            .OrderByDescending(score => score)
            .ToArray();

        if (log == null)
            return houses;

        try
        {
            houses = houses.CalculateStats(state, log);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Calculating stats for game {gameId} failed.", state.GameId);
        }
        return houses;
    }

    private static HouseScore GetHouseScore(int idx, House house, State state, string houseDataRaw, IGrouping<int, LogItem>[]? logsPerTurn, WildligKnowledge? wildligKnowledge)
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
            castles, cla, houseSpeed?.MinutesPerMove ?? 0, houseSpeed?.MovesCount ?? 0,
            battlesInTurn, turn,
            wildligKnowledge == null ? null :
            wildligKnowledge[house].Knows ? 1 : 1D / (9 - wildligKnowledge[house].KnownWildlings),
            new Stats(), new HousesInteractions(state.HousesOrder.Where(h => h != house)), new PlayersInteractions());
    }

    private static bool IsPlayerBattleLogItem(House house, LogItem item) =>
        item.Phase == Phase.March &&
        item.Message.Contains("Battle!") &&
        item.Message.Contains(house.ToString(), StringComparison.InvariantCultureIgnoreCase);
}