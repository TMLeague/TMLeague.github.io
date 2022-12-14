using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TMGameImporter.Configuration;
using TMGameImporter.Files;
using TMGameImporter.Http;
using TMGameImporter.Services;

Console.WriteLine("TMGameImporter started...");

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
            .AddScoped<ThroneMasterApi>()
            .AddScoped<PathProvider>()
            .AddScoped<FileLoader>()
            .AddScoped<FileSaver>()
            .AddScoped<GameImportingService>()
            .AddScoped<PlayerImportingService>()
            .AddScoped<DivisionImportingService>()
            .AddScoped<SeasonImportingService>()
            .AddScoped<LeagueImportingService>()
            .AddScoped<MainImportingService>())
    .Build();

var service = host.Services.GetRequiredService<MainImportingService>();
await service.Import();