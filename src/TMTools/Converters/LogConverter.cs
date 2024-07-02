using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Globalization;
using TMModels.ThroneMaster;

namespace TMTools.Converters;

public class LogConverter
{
    private readonly ILogger<LogConverter> _logger;

    public LogConverter(ILogger<LogConverter> logger)
    {
        _logger = logger;
    }

    public Log? Convert(int gameId, string htmlString)
    {
        var html = new HtmlDocument();
        html.LoadHtml(htmlString);

        if (html.DocumentNode.InnerText.Contains("ERROR: Invalid Game ID!"))
        {
            _logger.LogWarning($"Log {gameId} skipped, because it's invalid game ID.");
            return null;
        }

        var nameNode = html.DocumentNode.SelectSingleNode("//body/h4");
        var name = GetText(nameNode);
        name = name[(name.IndexOf(":", StringComparison.InvariantCulture) + 3)..^1];

        var createdNode = html.DocumentNode.SelectSingleNode("//body").ChildNodes
            .Last(node => node.NodeType == HtmlNodeType.Element);
        var created = DateTime.MinValue;
        if (createdNode.InnerText.StartsWith("Created "))
        {
            created = DateTime.Parse(createdNode.InnerText[8..28], CultureInfo.InvariantCulture);
            var zone = int.Parse(createdNode.InnerText[32..]);
            created = created.AddHours(-zone);
        }

        var proNode = html.DocumentNode.SelectSingleNode("/html[1]/body[1]/table[1]/tr[1]/td[2]/table[2]/tr[1]/th[1]");
        var isProfessional = !proNode.InnerText.Contains("UNRATED GAME");

        var settings = GetSettings(html);
        var logItems = GetLog(html);

        return new Log(gameId, name, created, isProfessional, settings, logItems);
    }

    private static Settings GetSettings(HtmlDocument html)
    {
        var tableNode = html.DocumentNode.SelectSingleNode("/html[1]/body[1]/table[1]/tr[1]/td[2]/table[1]");
        var settings = new Settings();
        foreach (var settingsNode in tableNode.ChildNodes
                     .Where(sn => sn.NodeType == HtmlNodeType.Element))
        {
            var kvNodes = settingsNode.SelectNodes("./td");
            if (kvNodes == null || kvNodes.Count < 2)
                continue;

            settings.Add(GetText(kvNodes.First()), GetText(kvNodes.Last()) == "YES");
        }

        return settings;
    }

    private List<LogItem> GetLog(HtmlDocument html)
    {
        var tableNode = html.DocumentNode.SelectSingleNode("/html[1]/body[1]/table[1]/tr[1]/td[1]/table[1]");
        var logItems = new List<LogItem>();
        foreach (var settingsNode in tableNode.ChildNodes
                     .Where(sn => sn.NodeType == HtmlNodeType.Element)
                     .Skip(1))
        {
            var cellNodes = settingsNode.SelectNodes("./td|./th");
            if (cellNodes == null || cellNodes.Count < 6)
                continue;

            var id = GetNumber(cellNodes[0]);

            var turn = GetNumber(cellNodes[1]);
            if (turn == null)
            {
                _logger.LogWarning($"  Error! Invalid log turn: {cellNodes[1].InnerText}");
                continue;
            }

            var phaseText = GetText(cellNodes[2]).Replace(" ", "");
            if (!Enum.TryParse<Phase>(phaseText, true, out var phase))
            {
                _logger.LogWarning($"  Error! Invalid log phase: {cellNodes[2].InnerText}");
                continue;
            }

            var player = GetText(cellNodes[3]);
            var message = GetText(cellNodes[4]);
            var dateText = GetText(cellNodes[5]);
            DateTime? date = null;
            if (DateTime.TryParse(dateText, out var rawDate))
                date = rawDate;

            var logItem = new LogItem(id, turn.Value, phase, player, message, date);
            logItems.Add(logItem);
        }

        return logItems;
    }

    private static string GetText(HtmlNode node) =>
        Uri.UnescapeDataString(node.InnerText)
            .Replace("&#160;", "")
            .Replace("\u002B", "+")
            .Replace("\u0027", "'")
            .Replace("\u003E", ">")
            .Trim();

    private static int? GetNumber(HtmlNode node)
    {
        var text = GetText(node);
        if (int.TryParse(text, out var res))
            return res;

        return null;
    }
}