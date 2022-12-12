using TMLeague.ViewModels;

namespace TMLeague.Services;

public class SeasonService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SeasonService> _logger;

    public SeasonService(HttpClient httpClient, ILogger<SeasonService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public static Task<SeasonSummaryViewModel> GetSeasonSummaryVm(string leagueId, string seasonId, CancellationToken none)
    {
        return Task.FromResult(new SeasonSummaryViewModel());
    }
}