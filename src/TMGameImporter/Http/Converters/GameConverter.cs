using Microsoft.Extensions.Logging;
using System.Text.Json;
using TMModels;
using TMModels.ThroneMaster;

namespace TMGameImporter.Http.Converters
{
    internal class GameConverter
    {
        private readonly LogConverter _logConverter;
        private readonly ILogger<GameConverter> _logger;

        public GameConverter(LogConverter logConverter, ILogger<GameConverter> logger)
        {
            _logConverter = logConverter;
            _logger = logger;
        }

        public Game? Convert(uint gameId, string dataString, string chatString, string logHtmlString)
        {
            var stateRaw = JsonSerializer.Deserialize<State>(dataString);
            var chatRaw = JsonSerializer.Deserialize<State>(chatString);
            var log = _logConverter.Convert(gameId, logHtmlString);

            return null;
            //return new Game(gameId, );
        }
    }
}
