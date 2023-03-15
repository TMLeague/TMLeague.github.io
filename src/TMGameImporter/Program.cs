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
using TMGameImporter.Services.Games;
using TMGameImporter.Services.Import;
using TMGameImporter.Services.Summaries;

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
            .AddScoped<PlayerConverter>()
            .AddScoped<StateConverter>()
            .AddScoped<GameConverter>()
            .AddScoped<LogConverter>()
            .AddScoped<PathProvider>()
            .AddScoped<FileLoader>()
            .AddScoped<FileSaver>()
            .AddScoped<GameImportingService>()
            .AddScoped<PlayerImportingService>()
            .AddScoped<DivisionImportingService>()
            .AddScoped<SeasonImportingService>()
            .AddScoped<LeagueImportingService>()
            .AddScoped<MainImportingService>()
            .AddScoped<DivisionSummaryCalculatingService>()
            .AddScoped<SeasonSummaryCalculatingService>()
            .AddScoped<LeagueSummaryCalculatingService>()
            .AddScoped<SummaryCalculatingService>())
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
var options = host.Services.GetRequiredService<IOptions<ImporterOptions>>();
logger.LogInformation(
    "Importing program started with following arguments: {arguments}", 
    string.Join("", GetArgumentsString()));

//var fixingService = host.Services.GetRequiredService<FixingService>();
//await fixingService.FixHouses();
//return;

var mainImportingService = host.Services.GetRequiredService<MainImportingService>();
await mainImportingService.Import();

var summaryCalculatingService = host.Services.GetRequiredService<SummaryCalculatingService>();
await summaryCalculatingService.Calculate();

string[] GetArgumentsString() =>
    new[]
    {
        GetArgumentLine(nameof(options.Value.BaseLocation), options.Value.BaseLocation),
        GetArgumentLine(nameof(options.Value.FetchFinishedDivisions), options.Value.FetchFinishedDivisions),
        GetArgumentLine(nameof(options.Value.FetchFinishedGames), options.Value.FetchFinishedGames),
        GetArgumentLine(nameof(options.Value.League), options.Value.League),
        GetArgumentLine(nameof(options.Value.Season), options.Value.Season),
        GetArgumentLine(nameof(options.Value.Division), options.Value.Division)
    };

string GetArgumentLine(string name, object? value) => 
    $"{Environment.NewLine} - {name}: {value}";