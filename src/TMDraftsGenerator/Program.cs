using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Channels;
using TMApplication.Services;
using TMDraftsGenerator;
using TMModels;

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
            .Configure<DraftOptions>(context.Configuration)
            .AddSingleton<DraftService>()
            .AddSingleton<PlayerStatsService>())
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
var draftService = host.Services.GetRequiredService<DraftService>();
var playerStatsService = host.Services.GetRequiredService<PlayerStatsService>();
var options = host.Services.GetRequiredService<IOptions<DraftOptions>>();

logger.LogInformation(
    "Generating drafts started with following arguments:\r\n{arguments}",
    string.Join(Environment.NewLine, ArgumentsString()));

var directory = Directory.CreateDirectory(options.Value.ResultsPath);
var bestScores = new DraftScoresBag();

var channel = Channel.CreateBounded<DraftScore>(100);
_ = Task.Run(async () =>
{
    try
    {
        var resultsPath = Path.Combine(options.Value.ResultsPath, "results.txt");
        await using var resultsFile = File.CreateText(resultsPath);
        await resultsFile.WriteAsync(
            $"ID\tNeighborMin\tNeighborMax\tNeighborStd\tEnemyMin\tEnemyMax\tEnemyStd{Environment.NewLine}");
        while (!cts.Token.IsCancellationRequested)
        {
            var score = await channel.Reader.ReadAsync(cts.Token);
            await resultsFile.WriteAsync(
                $"{score.Id}\t{score.NeighborMin}\t{score.NeighborMax}\t{score.NeighborStd}\t{score.EnemyMin}\t{score.EnemyMax}\t{score.EnemyStd}{Environment.NewLine}");
            await resultsFile.FlushAsync();
            logger.LogInformation(
                $"[{DateTime.Now:HH:mm:ss}] Best scores ({bestScores.Count}): {string.Join(", ", bestScores.Values.OrderBy(s => s.NeighborStd).Select(draftScore => $"{draftScore.Id} ({Math.Round(draftScore.NeighborStd, 2)}, {Math.Round(draftScore.EnemyStd, 2)})"))}");
        }

        logger.LogInformation("Results writing task finished.");
    }
    catch (OperationCanceledException)
    {
        logger.LogWarning($"Results writing task cancelled.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Results writing task failed.");
    }
    finally
    {
        cts.Cancel();
    }
});

foreach (var file in directory.GetFiles("*.draft.txt"))
    file.Delete();

logger.LogInformation("Results directory files are removed.");

var players = Enumerable.Range(1, options.Value.Players).Select(i => i.ToString()).ToArray();

Enumerable.Range(0, options.Value.Threads).AsParallel().ForAll(async taskId =>
{
    logger.LogInformation($"Task {taskId} started.");
    try
    {
        var i = 0;
        while (!cts.Token.IsCancellationRequested)
        {
            var draft = draftService.GetDraft(options.Value.Players, options.Value.Houses);
            if (draft == null)
                break;

            var draftTable = draft.Table.Select(housesTemplate =>
                housesTemplate.Select(HouseParser.Parse).ToArray()).ToArray();
            var allStats = playerStatsService.GetStats(draftTable, players)
                .SelectMany(s => s).OfType<PlayerDraftStat>().ToArray();
            var score = new DraftScore($"{taskId}-{i}", allStats);

            if (!bestScores.IsDominated(score))
            {
                await bestScores.Add(score, cts.Token);

                var path = Path.Combine(options.Value.ResultsPath, $"{taskId}-{i}.draft.txt");
                _ = File.WriteAllTextAsync(path, draft.Serialize())
                    .ContinueWith(_ => logger.LogTrace($"Draft {taskId}-{i} saved."));

                await channel.Writer.WriteAsync(score, cts.Token);
            }

            ++i;
        }

        logger.LogInformation($"Task {taskId} finished.");
    }
    catch (OperationCanceledException)
    {
        logger.LogWarning($"Task {taskId} cancelled.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, $"Task {taskId} failed.");
    }
});

try
{
    await Task.Delay(TimeSpan.FromDays(14), cts.Token);
}
catch (TaskCanceledException)
{
    // Ignore
}

logger.LogInformation("Program finished.");

string[] ArgumentsString() =>
    new[]
    {
        ArgumentLine(nameof(options.Value.Players), options.Value.Players),
        ArgumentLine(nameof(options.Value.Houses), options.Value.Houses),
        ArgumentLine(nameof(options.Value.Threads), options.Value.Threads),
        ArgumentLine(nameof(options.Value.ResultsPath), options.Value.ResultsPath)
    };

string ArgumentLine(string name, object? value) =>
    $"{Environment.NewLine} - {name}: {value}";