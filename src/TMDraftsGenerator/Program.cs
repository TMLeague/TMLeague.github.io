using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TMApplication.Services;
using TMDraftsGenerator;
using TMModels;

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
await resultsFile.WriteAsync($"ID\tNeighborMin\tNeighborMax\tNeighborStd\tEnemyMin\tEnemyMax\tEnemyStd{Environment.NewLine}");

var players = Enumerable.Range(1, options.Value.Players).Select(i => i.ToString()).ToArray();

var bestScores = new HashSet<DraftScore>();

foreach (var (draft, i) in draftService.GetDrafts(options.Value.Players, options.Value.Houses).Select((d, i) => (d, i)))
{
    var draftTable = draft.Table.Select(housesTemplate =>
        housesTemplate.Select(HouseParser.Parse).ToArray()).ToArray();
    var allStats = playerStatsService.GetStats(draftTable, players)
        .SelectMany(s => s).OfType<PlayerDraftStat>().ToArray();
    var score = new DraftScore(i.ToString(), allStats);

    if (bestScores.All(draftScore => !draftScore.IsDominating(score)))
    {
        bestScores.RemoveWhere(draftScore => score.IsDominating(draftScore));
        bestScores.Add(score);

        var path = Path.Combine(options.Value.ResultsPath, $"{i}.draft.txt");
        _ = File.WriteAllTextAsync(path, draft.Serialize())
            .ContinueWith(_ => logger.LogTrace($"Draft {i} saved."));

        await resultsFile.WriteAsync(
            $"{i}\t{score.NeighborMin}\t{score.NeighborMax}\t{score.NeighborStd}\t{score.EnemyMin}\t{score.EnemyMax}\t{score.EnemyStd}{Environment.NewLine}");
        logger.LogInformation($"[{DateTime.Now:HH:mm:ss}] Best scores ({bestScores.Count}) : {string.Join(", ", bestScores.Select(draftScore => draftScore.Name).OrderBy(s => s))}");
    }
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