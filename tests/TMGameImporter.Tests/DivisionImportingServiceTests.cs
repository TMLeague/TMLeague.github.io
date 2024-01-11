using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using TMGameImporter.Services.Import;
using TMModels;

namespace TMGameImporter.Tests;
public class DivisionImportingServiceTests
{
    [Theory]
    [InlineData(
        11, 1, 12, 11, 30, 100, 2,
        10, 2, 13, 12, 33, 90, 0,
        "Wins,Cla,Supplies,PowerTokens,MinutesPerMove")]
    [InlineData(
        10, 2, 12, 11, 30, 100, 2,
        10, 1, 13, 12, 33, 90, 0,
        "Wins,Cla,Supplies,PowerTokens,MinutesPerMove")]
    [InlineData(
        10, 1, 13, 11, 30, 100, 2,
        10, 1, 12, 12, 33, 90, 0,
        "Wins,Cla,Supplies,PowerTokens,MinutesPerMove")]
    [InlineData(
        10, 1, 12, 12, 30, 100, 2,
        10, 1, 12, 11, 33, 90, 0,
        "Wins,Cla,Supplies,PowerTokens,MinutesPerMove")]
    [InlineData(
        10, 1, 12, 11, 33, 100, 2,
        10, 1, 12, 11, 30, 90, 0,
        "Wins,Cla,Supplies,PowerTokens,MinutesPerMove")]
    [InlineData(
        10, 1, 12, 11, 30, 90, 2,
        10, 1, 12, 11, 30, 100, 0,
        "Wins,Cla,Supplies,PowerTokens,MinutesPerMove")]
    [InlineData(
        10, 1, 12, 11, 30, 100, 0,
        10, 1, 12, 11, 30, 100, 2,
        "Wins,Cla,Supplies,PowerTokens,MinutesPerMove,Penalties")]
    [InlineData(
        10, 1, 12, 11, 33, 100, 2,
        10, 2, 13, 12, 30, 90, 0,
        "PowerTokens,Wins,Cla,Supplies,MinutesPerMove,Penalties")]
    public void Should_Be_Greater(double p1Points, int p1Wins, int p1Cla, int p1Supplies, int p1Pt, double p1Mpm,
        double p1Penalties, double p2Points, int p2Wins, int p2Cla, int p2Supplies, int p2Pt, double p2Mpm,
        double p2Penalties, string tiebreakersStr)
    {
        var p1 = GetPlayer(p1Points, p1Wins, p1Cla, p1Supplies, p1Pt, p1Mpm, p1Penalties);
        var p2 = GetPlayer(p2Points, p2Wins, p2Cla, p2Supplies, p2Pt, p2Mpm, p2Penalties);
        var tiebreakers = tiebreakersStr.Split(',').Select(Enum.Parse<Tiebreaker>);
        var result = DivisionImportingService.Compare(p1, p2, tiebreakers);
        result.Should().BePositive();
    }

    private static PlayerResult GetPlayer(double p1Points, int p1Wins, int p1Cla, int p1Supplies, int p1Pt,
        double p1Mpm, double p1Penalties) => new(
        "A", p1Points, p1Wins, p1Cla, p1Supplies, p1Pt, p1Mpm, 1, 
        Array.Empty<HouseResult>(), p1Penalties, Array.Empty<PlayerPenalty>(), new Stats());
}
