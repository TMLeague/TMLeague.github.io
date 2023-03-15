using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using TMGameImporter.Configuration;
using TMModels;

namespace TMGameImporter.Services.Games
{
    internal class GameFixingService
    {
        private readonly IOptions<ImporterOptions> _options;
        private readonly ILogger<GameFixingService> _logger;

        public GameFixingService(IOptions<ImporterOptions> options, ILogger<GameFixingService> logger)
        {
            _options = options;
            _logger = logger;
        }

        public async Task FixHouseName()
        {
            var games = Directory.GetFiles(_options.Value.BaseLocation);
            foreach (var gamePath in games)
            {
                var gameInfo = new FileInfo(gamePath);
                var gameId = gameInfo.Name.Split('.')[0];
                var gameStream = File.OpenRead(gamePath);
                var gameV1 = await JsonSerializer.DeserializeAsync<GameV1>(gameStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (gameV1 == null)
                {
                    _logger.LogWarning("Game {gameId} was not loaded correctly.", gameId);
                    continue;
                }

                var game = new Game(
                    gameV1.Id,
                    gameV1.Name,
                    gameV1.IsFinished,
                    gameV1.IsStalling,
                    gameV1.Turn,
                    gameV1.Map,
                    gameV1.Houses.Select(v1 => new HouseScore(
                        v1.Name,
                        v1.Player,
                        v1.Throne,
                        v1.Fiefdoms,
                        v1.KingsCourt,
                        v1.Supplies,
                        v1.PowerTokens,
                        v1.Strongholds,
                        v1.Castles,
                        v1.Cla,
                        v1.MinutesPerMove,
                        v1.Moves,
                        v1.BattlesInTurn,
                        v1.Stats)).ToArray(),
                    gameV1.GeneratedTime,
                    gameV1.IsCreatedManually);
                var content = JsonSerializer.Serialize(game,
                    new JsonSerializerOptions
                    {
                        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                        PropertyNameCaseInsensitive = true
                    });
                await File.WriteAllTextAsync(gamePath, content, CancellationToken.None);
                _logger.LogInformation("Game {gameId} saved.", gameId);
            }
        }

        private record GameV1(
            int Id,
            string Name,
            bool IsFinished,
            bool IsStalling,
            int Turn,
            Map Map,
            HouseScoreV1[] Houses,
            DateTimeOffset GeneratedTime,
            bool IsCreatedManually = false);

        private record HouseScoreV1(
            House Name,
            string Player,
            int Throne,
            int Fiefdoms,
            int KingsCourt,
            int Supplies,
            int PowerTokens,
            int Strongholds,
            int Castles,
            int Cla,
            double MinutesPerMove,
            int Moves,
            int[] BattlesInTurn,
            Stats? Stats) : IComparable<HouseScore>
        {
            public Stats Stats { get; } = Stats ?? new Stats();

            public int CompareTo(HouseScore? otherHouse)
            {
                if (otherHouse == null)
                    return 1;

                if (Castles + Strongholds != otherHouse.Castles + otherHouse.Strongholds)
                    return Castles + Strongholds - (otherHouse.Castles + otherHouse.Strongholds);

                if (Cla != otherHouse.Cla)
                    return Cla - otherHouse.Cla;

                if (Supplies != otherHouse.Supplies)
                    return Supplies - otherHouse.Supplies;

                return otherHouse.Throne - Throne;
            }
        }
    }
}
