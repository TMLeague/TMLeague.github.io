using FluentAssertions;
using TMApplication.Services;
using TMModels;

namespace TMApplication.Tests;

public class PlayerStatsServiceTests
{
    private readonly PlayerStatsService _sut;

    public PlayerStatsServiceTests()
    {
        _sut = new PlayerStatsService();
    }

    [Fact]
    public void GetStats_Should_Returns_Stats()
    {
        var draftTable = new[]
        {
            new [] { House.Baratheon, House.Lannister, House.Stark, House.Martell, House.Greyjoy, House.Tyrell, House.Unknown, House.Unknown, House.Unknown, House.Unknown },
            new [] { House.Martell, House.Unknown, House.Greyjoy, House.Lannister, House.Unknown, House.Unknown, House.Baratheon, House.Stark, House.Tyrell, House.Unknown },
            new [] { House.Lannister, House.Martell, House.Unknown, House.Unknown, House.Unknown, House.Stark, House.Unknown, House.Tyrell, House.Baratheon, House.Greyjoy },
            new [] { House.Unknown, House.Tyrell, House.Baratheon, House.Greyjoy, House.Stark, House.Unknown, House.Lannister, House.Unknown, House.Unknown, House.Martell },
            new [] { House.Stark, House.Baratheon, House.Lannister, House.Unknown, House.Tyrell, House.Unknown, House.Unknown, House.Martell, House.Greyjoy, House.Unknown },
            new [] { House.Unknown, House.Unknown, House.Unknown, House.Stark, House.Lannister, House.Greyjoy, House.Tyrell, House.Baratheon, House.Martell, House.Unknown },
            new [] { House.Greyjoy, House.Unknown, House.Unknown, House.Tyrell, House.Martell, House.Unknown, House.Stark, House.Lannister, House.Unknown, House.Baratheon },
            new [] { House.Unknown, House.Unknown, House.Tyrell, House.Baratheon, House.Unknown, House.Martell, House.Unknown, House.Greyjoy, House.Lannister, House.Stark },
            new [] { House.Tyrell, House.Stark, House.Martell, House.Unknown, House.Unknown, House.Baratheon, House.Greyjoy, House.Unknown, House.Unknown, House.Lannister },
            new [] { House.Unknown, House.Greyjoy, House.Unknown, House.Unknown, House.Baratheon, House.Lannister, House.Martell, House.Unknown, House.Stark, House.Tyrell }
        };

        var players = Enumerable.Range(0, 10).Select(i => $"P{i}").ToArray();

        var allStats = _sut.GetStats(draftTable, players).ToArray();

        foreach (var playerStats in allStats)
        {
            playerStats.GameMax.Should().Be(4);
            playerStats.Count(stat => stat == null).Should().Be(1);
        }
    }

    [Theory]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public void Neighbor_Should_Be_Symmetric(int playersCount)
    {
        foreach (var (house, houseNeighbors) in Houses.Neighbors[playersCount])
            foreach (var house2 in houseNeighbors)
                Houses.Neighbors[playersCount][house2].Should().Contain(house);
    }
}