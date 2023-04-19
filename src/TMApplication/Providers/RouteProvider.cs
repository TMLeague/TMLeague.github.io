using TMApplication.ViewModels;

namespace TMApplication.Providers;

public static class RouteProvider
{
    private const string League = "league";
    private const string Season = "season";
    private const string Division = "division";
    private const string Game = "game";

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
        $"{League}/{leagueId}/index";

    public static string GetLeagueSummaryRoute(string leagueId, string? divisionId = null, TableType? tableType = null, ScoreType? scoreType = null) =>
        scoreType == null ?
            (string.IsNullOrEmpty(divisionId) ?
                $"{League}/{leagueId}/summary" :
                $"{League}/{leagueId}/summary/{TableType.Players}/{ScoreType.Best}/{divisionId}") :
        string.IsNullOrEmpty(divisionId)
            ? $"{League}/{leagueId}/summary/{tableType}/{ScoreTypes.Get(tableType, scoreType)}"
            : $"{League}/{leagueId}/summary/{tableType}/{ScoreTypes.Get(tableType, scoreType)}/{divisionId}";

    public static string GetLeaguePlayersRoute(string leagueId) =>
        $"{League}/{leagueId}/players";

    public static string GetLeagueSeasonsRoute(string leagueId) =>
        $"{League}/{leagueId}/seasons";

    public static string GetLeagueSeasonRoute(string leagueId, string seasonId) =>
        $"{League}/{leagueId}/season/{seasonId}";

    public static string GetLeagueDivisionRoute(string leagueId, string seasonId, string divisionId) =>
        $"{League}/{leagueId}/season/{seasonId}/division/{divisionId}";

    public static string GetLeagueJudgeRoute(string leagueId) =>
        $"{League}/{leagueId}/judge";

    public static string GetGameRoute(string? leagueId, int gameId) =>
        leagueId == null ?
            GetGameRoute(gameId) :
            $"{League}/{leagueId}/{Game}/{gameId}";

    public static string GetGameRoute(int gameId) =>
        $"{Game}/{gameId}";
}