using System.Text.Json;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TMGameImporter.Http.Converters;
using TMModels;
using TMModels.ThroneMaster;

namespace TMGameImporter.Tests;

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
        var gameData = JsonSerializer.Deserialize<StateRaw>(
            File.ReadAllText(Path.Combine(dataDirectory, $"{gameId}_gamedata.json")),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var chat = JsonSerializer.Deserialize<StateRaw>(
            File.ReadAllText(Path.Combine(dataDirectory, $"{gameId}_chat.json")), 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var logHtml = File.ReadAllText(Path.Combine(dataDirectory, $"{gameId}_log.html"));

        // act
        var game = _gameConverter.Convert(gameId, gameData!, chat!, logHtml);

        // assert
        game.Should().NotBeNull();
        game!.Id.Should().Be(gameId);
        game.Name.Should().Be("PBEM - Silent League S5/D1/G3");
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

        game.Houses.Should().HaveCount(6);
        game.Houses[0].House.Should().Be(House.Martell);
        game.Houses[1].House.Should().Be(House.Stark);
        game.Houses[2].House.Should().Be(House.Greyjoy);
        game.Houses[3].House.Should().Be(House.Baratheon);
        game.Houses[4].House.Should().Be(House.Lannister);
        game.Houses[5].House.Should().Be(House.Tyrell);

        var baratheon = game.Houses.FirstOrDefault(house => house.House == House.Baratheon);
        baratheon!.PowerTokens.Should().Be(1);
        baratheon.BattlesInTurn[0].Should().Be(0);
        baratheon.BattlesInTurn[1].Should().Be(0);
        baratheon.BattlesInTurn[2].Should().Be(0);
        baratheon.BattlesInTurn[3].Should().Be(0);
        baratheon.BattlesInTurn[4].Should().Be(3);

        var lannister = game.Houses.FirstOrDefault(house => house.House == House.Lannister);
        lannister!.Player.Should().Be("Mandy Storm");
        lannister.BattlesInTurn[0].Should().Be(0);
        lannister.BattlesInTurn[1].Should().Be(0);
        lannister.BattlesInTurn[2].Should().Be(0);
        lannister.BattlesInTurn[3].Should().Be(0);
        lannister.BattlesInTurn[4].Should().Be(1);

        game.Houses.Length.Should().Be(6);
        var stark = game.Houses.FirstOrDefault(house => house.House == House.Stark);
        stark.Should().NotBeNull();
        stark!.Player.Should().Be("damrej");
        stark.Throne.Should().Be(3);
        stark.Fiefdoms.Should().Be(3);
        stark.KingsCourt.Should().Be(1);
        stark.Supplies.Should().Be(5);
        stark.PowerTokens.Should().Be(0);
        stark.Strongholds.Should().Be(2);
        stark.Castles.Should().Be(3);
        stark.Cla.Should().Be(11);
        stark.MinutesPerMove.Should().BeApproximately(120.4, 0.01);
        stark.Moves.Should().Be(71);
        stark.BattlesInTurn[0].Should().Be(0);
        stark.BattlesInTurn[1].Should().Be(2);
        stark.BattlesInTurn[2].Should().Be(2);
        stark.BattlesInTurn[3].Should().Be(1);
        stark.BattlesInTurn[4].Should().Be(1);

        var tyrell = game.Houses.FirstOrDefault(house => house.House == House.Tyrell);
        tyrell!.BattlesInTurn[0].Should().Be(0);
        tyrell.BattlesInTurn[1].Should().Be(1);
        tyrell.BattlesInTurn[2].Should().Be(2);
        tyrell.BattlesInTurn[3].Should().Be(2);
        tyrell.BattlesInTurn[4].Should().Be(2);

        var greyjoy = game.Houses.FirstOrDefault(house => house.House == House.Greyjoy);
        greyjoy!.BattlesInTurn[0].Should().Be(0);
        greyjoy.BattlesInTurn[1].Should().Be(2);
        greyjoy.BattlesInTurn[2].Should().Be(2);
        greyjoy.BattlesInTurn[3].Should().Be(1);
        greyjoy.BattlesInTurn[4].Should().Be(1);

        var martell = game.Houses.FirstOrDefault(house => house.House == House.Martell);
        martell!.BattlesInTurn[0].Should().Be(0);
        martell.BattlesInTurn[1].Should().Be(1);
        martell.BattlesInTurn[2].Should().Be(2);
        martell.BattlesInTurn[3].Should().Be(2);
        martell.BattlesInTurn[4].Should().Be(4);
    }
}