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

if (options.Value.QualityMeasures.List.All(measureOptions => !measureOptions.Enabled))
{
    logger.LogError("All quality measures are disabled. Enable at least one of them and restart the application.");
    return;
}

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
            $"ID\t{ResultsHeader(options.Value.QualityMeasures)}{Environment.NewLine}");
        while (!cts.Token.IsCancellationRequested)
        {
            var score = await channel.Reader.ReadAsync(cts.Token);
            await resultsFile.WriteAsync(
                $"{score.Id}\t{ResultsRow(score, options.Value.QualityMeasures)}{Environment.NewLine}");
            await resultsFile.FlushAsync();
            logger.LogInformation(
                $"[{DateTime.Now:HH:mm:ss}] Best scores ({bestScores.Count}): " +
                string.Join(", ", bestScores.Values
                    .OrderBy(s => s.Neighbor.Std)
                    .Select(draftScore => $"{draftScore.Id} ({ResultsBestScore(draftScore, options.Value.QualityMeasures)})")));
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
                housesTemplate.Select(Houses.Parse).ToArray()).ToArray();
            var allStats = playerStatsService.GetStats(draftTable, players)
                .SelectMany(s => s).OfType<PlayerDraftStat>().ToArray();
            var score = new DraftScore($"{taskId}-{i}", allStats);

            if (score.IsMeetingConstraints(options.Value.QualityMeasures) && !bestScores.IsDominated(score, options.Value.QualityMeasures))
            {
                await bestScores.Add(score, cts.Token, options.Value.QualityMeasures);

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
    await Task.Delay(Timeout.Infinite, cts.Token);
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
        ArgumentLine(nameof(options.Value.ResultsPath), options.Value.ResultsPath),
        ArgumentLine(nameof(options.Value.QualityMeasures), QualityMeasureNames(options.Value.QualityMeasures)),
    };

string QualityMeasureNames(QualityMeasures measures) => string.Join(", ",
    measures.List
        .Where(measureOptions => measureOptions.Enabled)
        .Select(measureOptions => measureOptions.Name));

string ArgumentLine(string name, object value) =>
    $"{Environment.NewLine} - {name}: {value}";

string ResultsHeader(QualityMeasures measures) => string.Join('\t',
    measures.List
        .Where(measureOptions => measureOptions.Enabled)
        .Select(measureOptions => $"{measureOptions.Name}Min\t{measureOptions.Name}Max\t{measureOptions.Name}Std"));

string ResultsRow(DraftScore score, QualityMeasures measures) => string.Join('\t',
    measures.List
        .Where(measureOptions => measureOptions.Enabled)
        .Select(measureOptions => $"{Math.Round(measureOptions.GetScore(score).Min, 2)}\t{Math.Round(measureOptions.GetScore(score).Max, 2)}\t{Math.Round(measureOptions.GetScore(score).Std, 2)}"));

string ResultsBestScore(DraftScore score, QualityMeasures measures) => string.Join(", ",
    measures.List
        .Where(measureOptions => measureOptions.Enabled)
        .Select(measureOptions => Math.Round(measureOptions.GetScore(score).Std, 2)));