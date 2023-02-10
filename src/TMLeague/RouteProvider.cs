namespace TMLeague;

internal class RouteProvider
{
    private const string League = "league";
    private const string Season = "season";
    private const string Division = "division";

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

    public static string GetLeagueSummaryRoute(string leagueId) => 
        $"{League}/{leagueId}/summary";

    public static string GetLeaguePlayersRoute(string leagueId) => 
        $"{League}/{leagueId}/players";

    public static string GetLeagueSeasonsRoute(string leagueId) => 
        $"{League}/{leagueId}/seasons";
}