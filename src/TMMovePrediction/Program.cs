// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TMTools.Converters;

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
            .AddScoped<LogConverter>()
            .AddScoped<ActionsConverter>())
    .Build();

var logConverter = host.Services.GetRequiredService<LogConverter>();
var actionsConverter = host.Services.GetRequiredService<ActionsConverter>();
var htmlString = await File.ReadAllTextAsync("C:\\Users\\mledzianowski\\Documents\\Offtopic\\GoT\\Thronemasters\\Data\\285854.html");
var log = logConverter.Convert(285854, htmlString);
if (log == null)
    return;

var actions = actionsConverter.Convert(log);

foreach (var action in actions.OrderBy(action => action.Player).ThenBy(action => action.Timestamp))
{
    Console.WriteLine($"{action.Player}\t{action.Phase}\t{action.Timestamp:dd.MM}\t{action.Timestamp:HH:mm}");
}