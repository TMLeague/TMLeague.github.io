using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TMGameImporter.Configuration;
using TMGameImporter.Files;
using TMGameImporter.Http;
using TMGameImporter.Http.Converters;
using TMGameImporter.Services.Import;
using TMGameImporter.Services.Summaries;

var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

var host = Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration(app =>
    {
        app.AddJsonFile("appsettings.json", true, true);
        if (!string.IsNullOrEmpty(environmentName))
            app.AddJsonFile($"appsettings.{environmentName}.json", true, true);
    })
    .ConfigureServices((context, services) =>
        services
            .AddLogging()
            .Configure<ImporterOptions>(context.Configuration.GetSection("Importer"))
            .AddScoped(_ => new HttpClient { BaseAddress = new Uri("https://game.thronemaster.net") })
            .AddScoped<IMemoryCache, MemoryCache>()
            .AddScoped<IThroneMasterDataProvider, ThroneMasterApi>()
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

var mainImportingService = host.Services.GetRequiredService<MainImportingService>();
await mainImportingService.Import();

var summaryCalculatingService = host.Services.GetRequiredService<SummaryCalculatingService>();
await summaryCalculatingService.Calculate();