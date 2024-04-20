using Microsoft.Extensions.Options;
using TMGameImporter.Configuration;

namespace TMGameImporter.Files;

internal class PathProvider
{
    private const string Leagues = "leagues";
    private const string Seasons = "seasons";
    private const string Divisions = "divisions";
    private const string Games = "games";
    private const string Interactions = "interactions";
    private const string Players = "players";
    private const string Results = "results";
    private const string Summary = "summary";

    private readonly IOptions<ImporterOptions> _options;

    public PathProvider(IOptions<ImporterOptions> options)
    {
        _options = options;
    }

    public string GetConfigFilePath(string? leagueId = null, string? seasonId = null, string? divisionId = null)
    {
        if (leagueId == null)
            return Path.Combine(_options.Value.BaseLocation, "home.json");

        if (seasonId == null)
            return Path.Combine(_options.Value.BaseLocation,
                Leagues, leagueId, $"{leagueId}.json");

        if (divisionId == null)
            return Path.Combine(_options.Value.BaseLocation,
                Leagues, leagueId, Seasons, seasonId, $"{seasonId}.json");

        return Path.Combine(_options.Value.BaseLocation,
            Leagues, leagueId, Seasons, seasonId, Divisions, $"{divisionId}.json");
    }

    public string GetGamePath(int gameId)
    {
        Directory.CreateDirectory(Path.Combine(_options.Value.BaseLocation, Games));
        return Path.Combine(_options.Value.BaseLocation, Games, $"{gameId}.json");
    }

    public string GetPlayerFilePath(string playerName)
    {
        Directory.CreateDirectory(Path.Combine(_options.Value.BaseLocation, Players));
        return Path.Combine(_options.Value.BaseLocation, Players, $"{playerName}.json");
    }

    public string GetPlayerAvatarPath(string playerName)
    {
        Directory.CreateDirectory(Path.Combine(_options.Value.BaseLocation, Players));
        return Path.Combine(_options.Value.BaseLocation, Players, $"{playerName}.jpeg");
    }

    public string GetResultsFilePath(string leagueId, string seasonId, string divisionId)
    {
        Directory.CreateDirectory(Path.Combine(_options.Value.BaseLocation, Results));
        Directory.CreateDirectory(Path.Combine(_options.Value.BaseLocation, Results, leagueId));
        Directory.CreateDirectory(Path.Combine(_options.Value.BaseLocation, Results, leagueId, seasonId));
        return Path.Combine(_options.Value.BaseLocation,
            Results, leagueId, seasonId, $"{divisionId}.json");
    }

    public string GetSummaryFilePath(string leagueId)
    {
        Directory.CreateDirectory(Path.Combine(_options.Value.BaseLocation, Results));
        Directory.CreateDirectory(Path.Combine(_options.Value.BaseLocation, Results, leagueId));
        return Path.Combine(_options.Value.BaseLocation,
            Results, leagueId, $"{Summary}.json");
    }

    public string GetLeagueInteractions(string leagueId)
    {
        Directory.CreateDirectory(Path.Combine(_options.Value.BaseLocation, Results));
        Directory.CreateDirectory(Path.Combine(_options.Value.BaseLocation, Results, leagueId));
        return Path.Combine(_options.Value.BaseLocation,
            Results, leagueId, $"{Interactions}.json");
    }

    public string GetDivisionInteractions(string leagueId, string seasonId, string divisionId)
    {
        Directory.CreateDirectory(Path.Combine(_options.Value.BaseLocation, Results));
        Directory.CreateDirectory(Path.Combine(_options.Value.BaseLocation, Results, leagueId));
        Directory.CreateDirectory(Path.Combine(_options.Value.BaseLocation, Results, leagueId, seasonId));
        return Path.Combine(_options.Value.BaseLocation,
            Results, leagueId, seasonId, $"{divisionId}.{Interactions}.json");
    }
}