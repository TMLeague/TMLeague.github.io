using FluentAssertions;

namespace TMModels.Tests;
public class InteractionsTests
{
    [Fact]
    public void Should_Add()
    {
        const string player1 = "P1";
        const string player2 = "P2";
        var totalInteractions = new TotalInteractions(new Dictionary<string, PlayersInteractions>()
        {
            [player1] = new(new Dictionary<string, PlayerInteractions>()
            {
                [player2] = new(player2)
                {
                    Interactions = new Interactions
                    {
                        Kills = new UnitStats(1, 0, 0, 0)
                    }
                }
            })
        }, new Dictionary<House, HousesInteractions>());

        var game = new Game(1, "a", true, false, 10,
            new Map(Array.Empty<Land>(), Array.Empty<Sea>(), Array.Empty<Port>()), null, null, new HouseScore[]
            {
                new(House.Baratheon, player1, 1, 1, 1, 3, 3, 2, 2, 5, 100, 20, Array.Empty<int>(), 10, 1,
                    new Stats(new BattleStats(), new UnitStats(2, 5, 3, 2), new UnitStats(), new PowerTokenStats(), new BidStats()), new HousesInteractions(),
                    new PlayersInteractions(new Dictionary<string, PlayerInteractions>
                    {
                        [player2] = new(player2)
                        {
                            Interactions = new Interactions
                            {
                                Kills = new UnitStats(1, 2, 1, 0)
                            }
                        }
                    }))
            },
            DateTimeOffset.Now);

        var newInteractions = totalInteractions + game;

        newInteractions.Players[player1][player2].Interactions.Kills.Footmen.Should().Be(2);
        newInteractions.Players[player1][player2].Interactions.Kills.Knights.Should().Be(2);
        newInteractions.Players[player1][player2].Interactions.Kills.SiegeEngines.Should().Be(1);
        newInteractions.Players[player1][player2].Interactions.Kills.Ships.Should().Be(0);
    }
}
