using FluentAssertions;
using System.Text.Json;
using TMModels;

namespace TMGameImporter.Tests;

public class DataProviderTests
{
    [Fact]
    public void Should_Load_Summary()
    {
        // arrange
        const string dataDirectory = "data";

        // act
        var raw = File.ReadAllText(Path.Combine(dataDirectory, "sl-summary.json"));
        var summary = JsonSerializer.Deserialize<Summary>(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // assert
        summary.Should().NotBeNull();
        summary!.Divisions[0].Players[0].Total.Stats.Should().NotBeNull();
        summary!.Divisions[0].Players[0].Total.Stats!.Battles.Won.Should().BeGreaterThan(0);
    }
}