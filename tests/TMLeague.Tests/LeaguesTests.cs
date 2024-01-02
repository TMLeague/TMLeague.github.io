using FluentAssertions;
using System.Text.Json;
using TMModels;
using Xunit.Abstractions;

namespace TMLeague.Tests;

public class LeaguesTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private const string HomePath = "home.json";
    private const string LeaguesPath = "leagues";
    private const string SeasonsPath = "seasons";
    private const string DivisionsPath = "divisions";
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    public LeaguesTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Data_Should_BeValid()
    {
        var home = JsonSerializer.Deserialize<Home>(File.OpenRead(HomePath), _jsonSerializerOptions);
        home.Should().NotBeNull();

        _testOutputHelper.WriteLine(
            "Home exists.");

        foreach (var leagueId in home!.Leagues)
            League_Should_BeValid(leagueId);
    }

    private void League_Should_BeValid(string leagueId)
    {
        var league = JsonSerializer.Deserialize<League>(
            File.OpenRead(LeaguePath(leagueId)), _jsonSerializerOptions);
        league.Should().NotBeNull();

        _testOutputHelper.WriteLine(
            $"League {leagueId.ToUpper()} exists.");

        league!.Name.Should().NotBeNull();
        foreach (var seasonId in league.AllSeasons)
            Season_Should_BeValid(leagueId, seasonId);
    }

    private void Season_Should_BeValid(string leagueId, string seasonId)
    {
        var season = JsonSerializer.Deserialize<Season>(
            File.OpenRead(SeasonPath(leagueId, seasonId)), _jsonSerializerOptions);
        season.Should().NotBeNull();

        _testOutputHelper.WriteLine(
            $"Season {leagueId.ToUpper()}/{seasonId.ToUpper()} exists.");

        season!.Name.Should().NotBeNull();
        foreach (var divisionId in season.Divisions)
            Division_Should_BeValid(leagueId, seasonId, divisionId);
    }

    private void Division_Should_BeValid(string leagueId, string seasonId, string divisionId)
    {
        var division = JsonSerializer.Deserialize<Division>(
            File.OpenRead(DivisionPath(leagueId, seasonId, divisionId)), _jsonSerializerOptions);
        division.Should().NotBeNull();

        _testOutputHelper.WriteLine(
            $"Division {leagueId.ToUpper()}/{seasonId.ToUpper()}/{divisionId.ToUpper()} exists.");

        division!.Name.Should().NotBeNull();
        division.Name.Should().NotBeNull();
        if (division.Penalties != null)
            foreach (var penalty in division.Penalties)
                Penalty_Should_BeValid(penalty, division.Players, division.Games);

        if (division.Replacements != null)
            foreach (var replacement in division.Replacements)
                Replacement_Should_BeValid(replacement, division.Players, division.Games);

        //if (division.IsFinished)
        //    foreach (var gameId in division.Games)
        //        gameId.Should().NotBeNull();

        _testOutputHelper.WriteLine(
            $"Division {leagueId.ToUpper()}/{seasonId.ToUpper()}/{divisionId.ToUpper()} is valid.");
    }

    private static void Penalty_Should_BeValid(Penalty penalty,
        IEnumerable<string> divisionPlayers, IEnumerable<int?> divisionGames)
    {
        divisionPlayers.Should().Contain(penalty.Player);
        if (penalty.Game != null)
            divisionGames.Should().Contain(penalty.Game);
        penalty.Points.Should().BePositive();
    }

    private static void Replacement_Should_BeValid(Replacement replacement,
        IEnumerable<string> divisionPlayers, IEnumerable<int?> divisionGames)
    {
        divisionPlayers.Should().Contain(replacement.From);
        divisionGames.Should().Contain(replacement.Game);
    }

    private static string LeaguePath(string leagueId) =>
        Path.Combine(LeaguesPath, leagueId, $"{leagueId}.json");

    private static string SeasonPath(string leagueId, string seasonId) =>
        Path.Combine(LeaguesPath, leagueId, SeasonsPath, seasonId, $"{seasonId}.json");

    private static string DivisionPath(string leagueId, string seasonId, string divisionId) =>
        Path.Combine(LeaguesPath, leagueId, SeasonsPath, seasonId, DivisionsPath, $"{divisionId}.json");
}