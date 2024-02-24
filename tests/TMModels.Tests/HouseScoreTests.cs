using FluentAssertions;

namespace TMModels.Tests;

public class HouseScoreTests
{
    [Fact]
    public void Should_Be_Sorted()
    {
        var baratheon = new HouseScore(House.Baratheon, "Bara", 1, 2, 3, 2, 5, 1, 0, 2, 0, 0, Array.Empty<int>(), 1, new Stats());
        var lannister = new HouseScore(House.Lannister, "Lanni", 2, 3, 1, 2, 5, 1, 0, 2, 0, 0, Array.Empty<int>(), 1, new Stats());
        var stark = new HouseScore(House.Stark, "Star", 3, 1, 2, 1, 5, 1, 1, 2, 0, 0, Array.Empty<int>(), 1, new Stats());

        stark.CompareTo(baratheon).Should().BeGreaterThan(0);
        stark.CompareTo(lannister).Should().BeGreaterThan(0);
        baratheon.CompareTo(lannister).Should().BeGreaterThan(0);
        baratheon.CompareTo(stark).Should().BeLessThan(0);
        lannister.CompareTo(stark).Should().BeLessThan(0);
        lannister.CompareTo(baratheon).Should().BeLessThan(0);

        var houses = new HouseScore[3];
        houses[0] = baratheon;
        houses[1] = lannister;
        houses[2] = stark;

        Array.Sort(houses);
        Array.Reverse(houses);

        houses[0].House.Should().Be(House.Stark);
        houses[1].House.Should().Be(House.Baratheon);
        houses[2].House.Should().Be(House.Lannister);
    }
}