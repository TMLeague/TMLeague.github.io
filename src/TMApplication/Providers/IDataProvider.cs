using TMModels;

namespace TMApplication.Providers;

public interface IDataProvider
{
    Task<Home?> GetHome(CancellationToken cancellationToken);
    Task<string[]?> GetPasswords(CancellationToken cancellationToken);
    Task<League?> GetLeague(string leagueId, CancellationToken cancellationToken);
    Task<Season?> GetSeason(string? leagueId, string seasonId, CancellationToken cancellationToken);
    Task<Division?> GetDivision(string? leagueId, string seasonId, string divisionId,
        CancellationToken cancellationToken);
    Task<Game?> GetGame(int gameId, CancellationToken cancellationToken);
    Task<Results?> GetResults(string leagueId, string seasonId, string divisionId, CancellationToken cancellationToken);
    Task<Draft[]> GetDrafts(int players, CancellationToken cancellationToken);
    Task<Summary?> GetSummary(string leagueId, CancellationToken cancellationToken);
    Task<Player?> GetPlayer(string playerName, CancellationToken cancellationToken);
};