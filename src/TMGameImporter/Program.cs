using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using TMGameImporter.Configuration;
using TMGameImporter.Files;
using TMGameImporter.Http;
using TMGameImporter.Http.Converters;
using TMGameImporter.Services.Import;
using TMGameImporter.Services.Summaries;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    Console.WriteLine("Canceling...");
    cts.Cancel();
    e.Cancel = true;
};

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
        services
            .AddLogging(builder => builder
                .AddSimpleConsole(options => options.SingleLine = true)
                .SetMinimumLevel(LogLevel.Debug))
            .Configure<ImporterOptions>(context.Configuration)
            .AddHttpClient<IThroneMasterDataProvider, ThroneMasterApi>((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<ImporterOptions>>();

                client.BaseAddress = new Uri(Consts.GameUrl);
                client.Timeout = TimeSpan.FromSeconds(5);

                if (!string.IsNullOrEmpty(options.Value.ApiAuthHeader))
                    client.DefaultRequestHeaders.Add("X-TMLeague-Auth", options.Value.ApiAuthHeader);

                if (!string.IsNullOrEmpty(options.Value.UserAgent))
                    client.DefaultRequestHeaders.Add("User-Agent", options.Value.UserAgent);
            })
            .ConfigurePrimaryHttpMessageHandler(provider =>
            {
                var options = provider.GetRequiredService<IOptions<ImporterOptions>>().Value;

                if (!string.IsNullOrEmpty(options.ApiAuthHeader) || string.IsNullOrEmpty(options.CfClearance))
                    return new HttpClientHandler();

                var handler = new HttpClientHandler
                {
                    UseCookies = true,
                    CookieContainer = new CookieContainer()
                };

                handler.CookieContainer.Add(
                    new Uri(Consts.GameUrl),
                    new Cookie("cf_clearance", options.CfClearance)
                );

                return handler;
            })
            .Services
            .AddScoped<IMemoryCache, MemoryCache>()
            //.AddScoped<FixingService>()
            .AddScoped<StateConverter>()
            .AddScoped<GameConverter>()
            .AddScoped<LogConverter>()
            .AddScoped<PathProvider>()
            .AddScoped<FileLoader>()
            .AddScoped<FileSaver>()
            .AddScoped<GameImportingService>()
            .AddScoped<DivisionImportingService>()
            .AddScoped<SeasonImportingService>()
            .AddScoped<LeagueImportingService>()
            .AddScoped<MainImportingService>()
            .AddScoped<DivisionSummaryCalculatingService>()
            .AddScoped<SeasonSummaryCalculatingService>()
            .AddScoped<LeagueSummaryCalculatingService>()
            .AddScoped<SummaryCalculatingService>()
            .AddScoped<PlayerCalculatingService>())
    .Build();

using var scope = host.Services.CreateScope();

var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
var options = scope.ServiceProvider.GetRequiredService<IOptions<ImporterOptions>>();
logger.LogInformation("Environment: {environment}", host.Services.GetRequiredService<IHostEnvironment>().EnvironmentName);
logger.LogInformation(
    "Importing program started with following arguments: {arguments}",
    string.Join("", ArgumentsString()));

//var fixingService = scope.ServiceProvider.GetRequiredService<FixingService>();
//await fixingService.FixHouses();
//return;

if (options.Value.Games is { Length: > 0 })
{
    var gameImportingService = scope.ServiceProvider.GetRequiredService<GameImportingService>();
    foreach (var gameId in options.Value.Games)
        await gameImportingService.Import(gameId, cts.Token);

    return;
}

var mainImportingService = scope.ServiceProvider.GetRequiredService<MainImportingService>();
await mainImportingService.Import(cts.Token);

var summaryCalculatingService = scope.ServiceProvider.GetRequiredService<SummaryCalculatingService>();
await summaryCalculatingService.Calculate(cts.Token);

var playerCalculatingService = scope.ServiceProvider.GetRequiredService<PlayerCalculatingService>();
await playerCalculatingService.Calculate(cts.Token);

string[] ArgumentsString() =>
    [
        ArgumentLine(nameof(options.Value.BaseLocation), options.Value.BaseLocation),
        ArgumentLine(nameof(options.Value.FetchFinishedDivisions), options.Value.FetchFinishedDivisions),
        ArgumentLine(nameof(options.Value.FetchFinishedGames), options.Value.FetchFinishedGames),
        ArgumentLine(nameof(options.Value.League), options.Value.League),
        ArgumentLine(nameof(options.Value.Season), options.Value.Season),
        ArgumentLine(nameof(options.Value.Seasons), options.Value.Seasons),
        ArgumentLine(nameof(options.Value.Division), options.Value.Division),
        ArgumentLine(nameof(options.Value.Games), options.Value.Games == null ? "" : string.Join(",", options.Value.Games)),
        ArgumentLine(nameof(options.Value.CfClearance), options.Value.CfClearance),
        ArgumentLine(nameof(options.Value.UserAgent), options.Value.UserAgent),
        ArgumentLine(nameof(options.Value.ApiAuthHeader), options.Value.ApiAuthHeader?.Length > 0 ? "***" : "")
    ];

string ArgumentLine(string name, object? value) =>
    $"{Environment.NewLine} - {name}: {value}";
