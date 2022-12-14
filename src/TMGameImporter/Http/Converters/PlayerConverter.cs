using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using TMModels;

namespace TMGameImporter.Http.Converters
{
    internal class PlayerConverter
    {
        private readonly ILogger<PlayerConverter> _logger;

        public PlayerConverter(ILogger<PlayerConverter> logger)
        {
            _logger = logger;
        }

        public Task<(Player, string? avatarUri)> Convert(string playerName, string htmlString)
        {
            var html = new HtmlDocument();
            html.LoadHtml(htmlString);

            var flagNode = html.DocumentNode.SelectNodes("//img[contains(@src, 'flags')]")?.FirstOrDefault();
            var country = string.Empty;
            if (flagNode == null)
                _logger.LogWarning("Country node not found!");
            else
                country = flagNode.GetAttributeValue("title", string.Empty);

            var rankPointsNode = html.DocumentNode.SelectNodes("//big[. = 'Rank Points']/parent::node()/parent::node()/th/big/a")?.FirstOrDefault();
            uint rankPoints = 0;
            if (rankPointsNode == null)
                _logger.LogWarning("Rank points node not found!");
            else
                uint.TryParse(rankPointsNode.InnerText, out rankPoints);

            var locationNode = html.DocumentNode.SelectNodes("//td[. = 'Location']/parent::node()/td[2]")?.FirstOrDefault();
            var location = string.Empty;
            if (locationNode == null)
                _logger.LogWarning("Location node not found!");
            else
                location = locationNode.InnerText;

            var speedNode = html.DocumentNode.SelectNodes("//td[. = 'Response Time PBEM']/parent::node()/td[2]")?.FirstOrDefault();
            var speed = string.Empty;
            if (speedNode == null)
                _logger.LogWarning("Speed node not found!");
            else
                speed = speedNode.InnerText;

            var avatarNode = html.DocumentNode.SelectNodes("//img[contains(@src, 'avatars')]")?.FirstOrDefault();
            string? avatarUri = null;
            if (avatarNode == null)
                _logger.LogWarning("Avatar node not found!");
            else
                avatarUri = avatarNode.GetAttributeValue("src", null);

            return Task.FromResult((new Player(playerName, rankPoints, country, location, speed), avatarUri));
        }
    }
}
