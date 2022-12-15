using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TMGameImporter.Http.Converters;
using TMModels;

namespace TMGameImporter.Tests
{
    public class GameConverterTests
    {
        private readonly GameConverter _gameConverter;

        public GameConverterTests()
        {
            var logConverter = new LogConverter(A.Fake<ILogger<LogConverter>>());
            var stateConverter = new StateConverter(A.Fake<ILogger<StateConverter>>());
            _gameConverter = new GameConverter(logConverter, stateConverter, A.Fake<ILogger<GameConverter>>());
        }

        [Fact]
        public void Should_Returns_Game()
        {
            // arrange
            const string dataDirectory = "data";
            const int gameId = 278340;
            var gameData = File.ReadAllText(Path.Combine(dataDirectory, $"{gameId}-gameData.json"));
            var chat = File.ReadAllText(Path.Combine(dataDirectory, $"{gameId}-chat.json"));
            var logHtml = File.ReadAllText(Path.Combine(dataDirectory, $"{gameId}-log.html"));

            // act
            var game = _gameConverter.Convert(gameId, gameData, chat, logHtml);

            // assert
            game.Should().NotBeNull();
            game!.Id.Should().Be(gameId);
            game.IsFinished.Should().BeTrue();
            game.IsStalling.Should().BeFalse();
            game.Turn.Should().Be(10);

            var moatCailin = game.Map.Lands.FirstOrDefault(land => land.Name == "Moat Cailin");
            moatCailin.Should().NotBeNull();
            moatCailin!.House.Should().Be(House.Stark);
            moatCailin.Footmen.Should().Be(2);
            moatCailin.Knights.Should().Be(1);
            moatCailin.SiegeEngines.Should().Be(1);
            moatCailin.Tokens.Should().Be(0);
            moatCailin.MobilizationPoints.Should().Be(1);
            moatCailin.Supplies.Should().Be(0);
            moatCailin.Crowns.Should().Be(0);

            var dragonstone = game.Map.Lands.FirstOrDefault(land => land.Name == "Dragonstone");
            dragonstone.Should().NotBeNull();
            dragonstone!.House.Should().Be(House.Baratheon);
            dragonstone.Footmen.Should().Be(1);
            dragonstone.Knights.Should().Be(1);
            dragonstone.SiegeEngines.Should().Be(0);
            dragonstone.Tokens.Should().Be(2);
            dragonstone.MobilizationPoints.Should().Be(2);
            dragonstone.Supplies.Should().Be(1);
            dragonstone.Crowns.Should().Be(1);

            var redwyneStraights = game.Map.Seas.FirstOrDefault(sea => sea.Name == "Redwyne Straights");
            redwyneStraights.Should().NotBeNull();
            redwyneStraights!.House.Should().Be(House.Martell);
            redwyneStraights.Ships.Should().Be(3);

            var portOfDragonstone = game.Map.Ports.FirstOrDefault(port => port.Name == "Port of Dragonstone");
            portOfDragonstone.Should().NotBeNull();
            portOfDragonstone!.Ships.Should().Be(1);
        }
    }
}