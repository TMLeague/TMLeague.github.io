using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using TMGameImporter.Configuration;
using TMModels;

namespace TMGameImporter.Services.Games
{
    internal class FixingService
    {
        private readonly IOptions<ImporterOptions> _options;
        private readonly ILogger<FixingService> _logger;

        public FixingService(IOptions<ImporterOptions> options, ILogger<FixingService> logger)
        {
            _options = options;
            _logger = logger;
        }

        public async Task FixHouses()
        {
            await UpdateAll<Game>(Path.Combine(_options.Value.BaseLocation, "games"));
            foreach (var directoryPath in Directory.GetDirectories(Path.Combine(_options.Value.BaseLocation, "results", "sl")))
                await UpdateAll<Results>(directoryPath);
            await UpdateAll<Summary>(Path.Combine(_options.Value.BaseLocation, "results", "sl"));
            foreach (var directoryPath in Directory.GetDirectories(Path.Combine(_options.Value.BaseLocation, "results", "loiaf")))
                await UpdateAll<Results>(directoryPath);
            await UpdateAll<Summary>(Path.Combine(_options.Value.BaseLocation, "results", "loiaf"));
        }

        private async Task UpdateAll<T>(string directory)
        {
            var files = Directory.GetFiles(directory);
            foreach (var path in files)
                await Update<T>(path);
        }

        private async Task Update<T>(string path)
        {
            var fileInfo = new FileInfo(path);
            var value = await Load<T>(path);
            if (value == null)
            {
                _logger.LogWarning("File {name} was not loaded correctly.", fileInfo.Name);
                return;
            }

            var content = JsonSerializer.Serialize(value,
                new JsonSerializerOptions
                {
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                    PropertyNameCaseInsensitive = true
                });

            await File.WriteAllTextAsync(path, content, CancellationToken.None);
            _logger.LogInformation("File {name} saved.", fileInfo.Name);
        }

        private static async Task<T?> Load<T>(string gamePath)
        {
            await using var gameStream = File.OpenRead(gamePath);
            var game = await JsonSerializer.DeserializeAsync<T>(gameStream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return game;
        }
    }
}
