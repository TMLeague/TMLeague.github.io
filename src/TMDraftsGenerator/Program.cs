using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TMApplication.Services;
using TMDraftsGenerator;
using TMModels;
using TMModels.Extensions;

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
            .Configure<DraftOptions>(context.Configuration)
            .AddSingleton<DraftService>()
            .AddSingleton<PlayerStatsService>())
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
var draftService = host.Services.GetRequiredService<DraftService>();
var playerStatsService = host.Services.GetRequiredService<PlayerStatsService>();
var options = host.Services.GetRequiredService<IOptions<DraftOptions>>();
logger.LogInformation(
    "Generating drafts started with following arguments: {arguments}",
    string.Join("", ArgumentsString()));

var directory = Directory.CreateDirectory(options.Value.ResultsPath);
foreach (var file in directory.GetFiles("*.draft.txt"))
    file.Delete();

logger.LogInformation("Results directory files are removed.");

var resultsPath = Path.Combine(options.Value.ResultsPath, "results.draft.txt");
await using var resultsFile = File.CreateText(resultsPath);

var players = Enumerable.Range(1, options.Value.Players).Select(i => i.ToString()).ToArray();

int bestIdx = 0;
double bestScore = int.MinValue;

logger.LogDebug("test");

foreach (var (draft, i) in draftService.GetDrafts(options.Value.Players, options.Value.Houses).Select((d, i) => (d, i)))
{
    var path = Path.Combine(options.Value.ResultsPath, $"{i}.draft.txt");
    _ = File.WriteAllTextAsync(path, draft.Serialize())
        .ContinueWith(_ => logger.LogTrace($"Draft {i} saved."));

    var draftTable = draft.Table.Select(housesTemplate =>
        housesTemplate.Select(HouseParser.Parse).ToArray()).ToArray();
    var allStats = playerStatsService.GetStats(draftTable, players)
        .SelectMany(s => s).OfType<PlayerDraftStat>().ToArray();
    var neighborMin = allStats.Select(stat => stat.Neighbor).Min();
    var neighborMax = allStats.Select(stat => stat.Neighbor).Max();
    var neighborStd = allStats.Select(stat => stat.Neighbor).ToArray().Std();
    var enemyMin = allStats.Select(stat => stat.Enemy).Min();
    var enemyMax = allStats.Select(stat => stat.Enemy).Max();
    var enemyStd = allStats.Select(stat => stat.Enemy).ToArray().Std();
    var score = neighborMin * options.Value.NeighborMin +
                neighborMax * options.Value.NeighborMax +
                neighborStd * options.Value.NeighborStd +
                enemyMin * options.Value.EnemyMin +
                enemyMax * options.Value.EnemyMax +
                enemyStd * options.Value.EnemyStd;
    if (score > bestScore)
    {
        bestIdx = i;
        bestScore = score;
    }

    await resultsFile.WriteAsync($"{i}|{neighborMin}|{neighborMax}|{neighborStd}|{enemyMin}|{enemyMax}|{enemyStd}|{score}{Environment.NewLine}");
    logger.LogDebug(
        $"Draft[{i}] calculated. Neighbors: {neighborMin}-{neighborMax} ({neighborStd:0.00}). Enemies: {enemyMin}-{enemyMax} ({enemyStd:0.00}). Score: {score:0.00} (best[{bestIdx}]: {bestScore:0.00}).");
}

string[] ArgumentsString() =>
    new[]
    {
        ArgumentLine(nameof(options.Value.Players), options.Value.Players),
        ArgumentLine(nameof(options.Value.Houses), options.Value.Houses),
        ArgumentLine(nameof(options.Value.ResultsPath), options.Value.ResultsPath),
        ArgumentLine(nameof(options.Value.NeighborMin), options.Value.NeighborMin),
        ArgumentLine(nameof(options.Value.NeighborMax), options.Value.NeighborMax),
        ArgumentLine(nameof(options.Value.NeighborStd), options.Value.NeighborStd),
        ArgumentLine(nameof(options.Value.EnemyMin), options.Value.EnemyMin),
        ArgumentLine(nameof(options.Value.EnemyMax), options.Value.EnemyMax),
        ArgumentLine(nameof(options.Value.EnemyStd), options.Value.EnemyStd)
    };

string ArgumentLine(string name, object? value) =>
    $"{Environment.NewLine} - {name}: {value}";