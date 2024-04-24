using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

var host = Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration(app =>
    {
        app.AddJsonFile("appsettings.json", true, true);
        if (!string.IsNullOrEmpty(environmentName))
            app.AddJsonFile($"appsettings.{environmentName}.json", true, true);
        app.AddCommandLine(args);
    })
    .ConfigureServices((context, services) =>
        services
            .AddLogging(builder => builder.AddSimpleConsole(options => options.SingleLine = true))
            .Configure<ImporterOptions>(context.Configuration)
            .AddScoped(_ => new HttpClient { BaseAddress = new Uri("https://game.thronemaster.net"), Timeout = TimeSpan.FromSeconds(5) })
            .AddScoped<IMemoryCache, MemoryCache>()
            .AddScoped<IThroneMasterDataProvider, ThroneMasterApi>()
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

var logger = host.Services.GetRequiredService<ILogger<Program>>();
var options = host.Services.GetRequiredService<IOptions<ImporterOptions>>();
logger.LogInformation(
    "Importing program started with following arguments: {arguments}",
    string.Join("", ArgumentsString()));

//var fixingService = host.Services.GetRequiredService<FixingService>();
//await fixingService.FixHouses();
//return;

if (options.Value.Games is { Length: > 0 })
{
    var gameImportingService = host.Services.GetRequiredService<GameImportingService>();
    foreach (var game in options.Value.Games)
        await gameImportingService.Import(game, cts.Token);
    return;
}

var mainImportingService = host.Services.GetRequiredService<MainImportingService>();
await mainImportingService.Import(cts.Token);

var summaryCalculatingService = host.Services.GetRequiredService<SummaryCalculatingService>();
await summaryCalculatingService.Calculate(cts.Token);

var playerCalculatingService = host.Services.GetRequiredService<PlayerCalculatingService>();
await playerCalculatingService.Calculate(cts.Token);

string[] ArgumentsString() =>
    new[]
    {
        ArgumentLine(nameof(options.Value.BaseLocation), options.Value.BaseLocation),
        ArgumentLine(nameof(options.Value.FetchFinishedDivisions), options.Value.FetchFinishedDivisions),
        ArgumentLine(nameof(options.Value.FetchFinishedGames), options.Value.FetchFinishedGames),
        ArgumentLine(nameof(options.Value.League), options.Value.League),
        ArgumentLine(nameof(options.Value.Season), options.Value.Season),
        ArgumentLine(nameof(options.Value.Seasons), options.Value.Seasons),
        ArgumentLine(nameof(options.Value.Division), options.Value.Division),
        ArgumentLine(nameof(options.Value.Games), options.Value.Games == null ? "" : string.Join(",", options.Value.Games))
    };

string ArgumentLine(string name, object? value) =>
    $"{Environment.NewLine} - {name}: {value}";