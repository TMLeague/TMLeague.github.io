namespace TMLeague.Services
{
    public class SeasonService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SeasonService> _logger;

        public SeasonService(HttpClient httpClient, ILogger<SeasonService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
    }
}
