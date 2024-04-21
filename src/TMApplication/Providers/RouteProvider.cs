using TMApplication.ViewModels;

namespace TMApplication.Providers;

public static class RouteProvider
{
    private const string Division = "division";
    private const string Divisions = "divisions";
    private const string Game = "game";
    private const string Index = "index";
    private const string Judge = "judge";
    private const string League = "league";
    private const string Leagues = "leagues";
    private const string Configuration = "configuration";
    private const string Player = "player";
    private const string Season = "season";
    private const string Seasons = "seasons";
    private const string Summary = "summary";
    private const string Interactions = "interactions";

    public static string GetRoute(string? leagueId = null, string? seasonId = null, string? divisionId = null)
    {
        if (leagueId == null)
            return "";

        if (seasonId == null)
            return $"{League}/{leagueId}";

        if (divisionId == null)
            return $"{League}/{leagueId}/{Season}/{seasonId}";

        return $"{League}/{leagueId}/{Season}/{seasonId}/{Division}/{divisionId}";
    }

    public static string GetLeagueIndexRoute(string leagueId) =>
        $"{League}/{leagueId}/{Index}";

    public static string GetLeagueSummaryRoute(string leagueId, string? divisionId = null, TableType? tableType = null, ScoreType? scoreType = null) =>
        scoreType == null ?
            (string.IsNullOrEmpty(divisionId) ?
                $"{League}/{leagueId}/{Summary}" :
                $"{League}/{leagueId}/{Summary}/{TableType.Players.ToString().ToLower()}/{ScoreType.Best.ToString().ToLower()}/{divisionId}") :
        string.IsNullOrEmpty(divisionId)
            ? $"{League}/{leagueId}/{Summary}/{tableType?.ToString().ToLower()}/{ScoreTypes.Get(tableType, scoreType)?.ToString().ToLower()}"
            : $"{League}/{leagueId}/{Summary}/{tableType?.ToString().ToLower()}/{ScoreTypes.Get(tableType, scoreType)?.ToString().ToLower()}/{divisionId}";

    public static object GetLeagueInteractionsRoute(string leagueId, string? divisionId = null, TableType? tableType = null, ScoreType? scoreType = null) =>
        scoreType == null ?
            (string.IsNullOrEmpty(divisionId) ?
                $"{League}/{leagueId}/{Interactions}" :
                $"{League}/{leagueId}/{Interactions}/{TableType.Players.ToString().ToLower()}/{ScoreType.Average.ToString().ToLower()}/{divisionId}") :
            string.IsNullOrEmpty(divisionId)
                ? $"{League}/{leagueId}/{Interactions}/{tableType?.ToString().ToLower()}/{ScoreTypes.Get(tableType, scoreType)?.ToString().ToLower()}"
                : $"{League}/{leagueId}/{Interactions}/{tableType?.ToString().ToLower()}/{ScoreTypes.Get(tableType, scoreType)?.ToString().ToLower()}/{divisionId}";

    public static string GetPlayerRoute(string? leagueId, string playerName, PlayerTableType type = PlayerTableType.Seasons) =>
        leagueId == null ?
            GetPlayerRoute(playerName, type) :
            $"{League}/{leagueId}/{Player}/{playerName}/{type.ToString().ToLower()}";

    public static string GetPlayerRoute(string playerName, PlayerTableType type = PlayerTableType.Seasons) =>
            $"{Player}/{playerName}/{type.ToString().ToLower()}";

    public static string GetLeagueSeasonsRoute(string leagueId) =>
        $"{League}/{leagueId}/{Seasons}";

    public static string GetLeagueSeasonRoute(string leagueId, string seasonId) =>
        $"{League}/{leagueId}/{Season}/{seasonId}";

    public static string GetLeagueDivisionRoute(string leagueId, string seasonId, string divisionId) =>
        $"{League}/{leagueId}/{Season}/{seasonId}/{Division}/{divisionId}";

    public static string GetLeagueDivisionCreationRoute(string leagueId, string? seasonNumber, string? divisionNumber, string? judge, IEnumerable<string>? playerNames)
    {
        var url = $"{League}/{leagueId}/{Configuration}";

        var query = new List<KeyValuePair<string, string>>();
        if (!string.IsNullOrWhiteSpace(seasonNumber))
            query.Add(new KeyValuePair<string, string>("season", seasonNumber));
        if (!string.IsNullOrWhiteSpace(divisionNumber))
            query.Add(new KeyValuePair<string, string>("division", divisionNumber));
        if (!string.IsNullOrWhiteSpace(judge))
            query.Add(new KeyValuePair<string, string>("judge", judge));
        if (playerNames != null)
            query.AddRange(playerNames.Select(name => new KeyValuePair<string, string>("player", name)));

        return query.Count == 0 ? url : $"{url}?{string.Join("&", query.Select(pair => $"{pair.Key}={pair.Value}"))}";
    }

    public static string GetLeagueDivisionConfigurationRoute(string leagueId, string seasonId, string divisionId) =>
        $"{League}/{leagueId}/{Season}/{seasonId}/{Division}/{divisionId}/{Configuration}";

    public static string GetLeagueDivisionFinishingRoute(string leagueId, string seasonId, string divisionId) =>
        $"{League}/{leagueId}/{Season}/{seasonId}/{Division}/{divisionId}/{Configuration}/finished";

    public static string GetLeagueJudgeRoute(string leagueId) =>
        $"{League}/{leagueId}/{Judge}";

    public static string GetGameRoute(string? leagueId, int gameId) =>
        leagueId == null ?
            GetGameRoute(gameId) :
            $"{League}/{leagueId}/{Game}/{gameId}";

    private static string GetGameRoute(int gameId) =>
        $"{Game}/{gameId}";

    public static string GetGithubDivision(string? leagueId, string? seasonId, string? divisionId, bool edit = true) =>
        string.IsNullOrWhiteSpace(seasonId) || string.IsNullOrWhiteSpace(divisionId)
            ? $"https://github.com/TMLeague/TMLeague.github.io/new/master/src/TMLeague/wwwroot/data/{Leagues}/{leagueId}/{Seasons}"
            : $"https://github.com/TMLeague/TMLeague.github.io/{(edit ? "edit" : "blob")}/master/src/TMLeague/wwwroot/data/{Leagues}/{leagueId}/{Seasons}/{seasonId}/{Divisions}/{divisionId}.json";
}