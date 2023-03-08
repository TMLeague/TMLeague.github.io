using System.Text.RegularExpressions;
using TMModels;
using TMModels.ThroneMaster;

namespace TMGameImporter.Http.Converters;

internal static class StatsConverter
{
    public static HouseScore[] CalculateStats(this HouseScore[] houseScores, Log log)
    {
        House? movingHouse = null;
        House? fightingHouse = null;
        foreach (var logItem in log.Logs)
        {
            switch (logItem.Phase)
            {
                case Phase.Planning:
                case Phase.Raven:
                case Phase.GameEnd:
                    break;
                case Phase.Raid:
                    houseScores.CountRaid(logItem);
                    break;
                case Phase.March:
                    movingHouse = logItem.House;
                    break;
                case Phase.Battle:
                    if (logItem.Message.Contains("to lead his forces."))
                        fightingHouse = GetFightingHouse(fightingHouse, movingHouse, logItem);
                    if (logItem.Message.Contains("lost the battle"))
                        houseScores.CountBattle(movingHouse ?? House.Unknown, fightingHouse ?? House.Unknown, logItem);
                    break;
                case Phase.Power:
                    if (logItem.Message.Contains("New Power Tokens"))
                        houseScores.CountConsolidatePowerOrder(logItem);
                    if (logItem.Message.Contains("Houses consolidate new power"))
                        houseScores.CountPowerConsolidation(logItem);
                    break;
                case Phase.Westeros:
                    if (logItem.Message.Contains("Game of Thrones - Houses consolidated new powers"))
                        houseScores.CountGameOfThrones(logItem);
                    if (logItem.Message.Contains("for the Iron Throne track."))
                        houseScores.CountIronThrone(logItem);
                    if (logItem.Message.Contains("for the Fiefdoms track."))
                        houseScores.CountFiefdoms(logItem);
                    if (logItem.Message.Contains("for the King's Court track."))
                        houseScores.CountKingsCourt(logItem);
                    if (logItem.Message.Contains("against the wildlings."))
                        houseScores.CountWildlings(logItem);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return houseScores;
    }

    private static void CountRaid(this HouseScore[] houseScores, LogItem logItem)
    {
        if (logItem.Message.Contains("raided the Consolidate Power"))
            ++houseScores.Get(logItem.House).Stats.PowerTokens.Raids;
    }

    private static House? GetFightingHouse(House? fightingHouse, House? movingHouse, LogItem logItem) =>
        logItem.House != movingHouse ?
            logItem.House :
            fightingHouse;

    public static void CountBattle(this HouseScore[] houseScores, House movingHouse, House fightingHouse, LogItem logItem)
    {
        var looser = logItem.House;
        var winner = movingHouse == looser ? fightingHouse : movingHouse;
        houseScores.Get(winner).Stats.Battles.Won++;
        houseScores.Get(looser).Stats.Battles.Lost++;
        var footmenMatch = Regex.Match(logItem.Message, @"(\d+) Footm\wn");
        if (footmenMatch.Success)
        {
            if (int.TryParse(footmenMatch.Groups[1].Value, out var footmen))
            {
                houseScores.Get(winner).Stats.Kills.Footmen += footmen;
                houseScores.Get(looser).Stats.Casualties.Footmen += footmen;
            }
        }
        var knightsMatch = Regex.Match(logItem.Message, @"(\d+) Knight");
        if (knightsMatch.Success)
        {
            if (int.TryParse(knightsMatch.Groups[1].Value, out var knights))
            {
                houseScores.Get(winner).Stats.Kills.Knights += knights;
                houseScores.Get(looser).Stats.Casualties.Knights += knights;
            }
        }
        var shipsMatch = Regex.Match(logItem.Message, @"(\d+) Ship");
        if (shipsMatch.Success)
        {
            if (int.TryParse(shipsMatch.Groups[1].Value, out var ships))
            {
                houseScores.Get(winner).Stats.Kills.Ships += ships;
                houseScores.Get(looser).Stats.Casualties.Ships += ships;
            }
        }
        var siegeEnginesMatch = Regex.Match(logItem.Message, @"(\d+) Siege");
        if (siegeEnginesMatch.Success)
        {
            if (int.TryParse(siegeEnginesMatch.Groups[1].Value, out var siegeEngines))
            {
                houseScores.Get(winner).Stats.Kills.SiegeEngines += siegeEngines;
                houseScores.Get(looser).Stats.Casualties.SiegeEngines += siegeEngines;
            }
        }
    }

    private static void CountConsolidatePowerOrder(this HouseScore[] houseScores, LogItem logItem)
    {
        if (int.TryParse(logItem.Message[^1].ToString(), out var tokens))
            houseScores.Get(logItem.House).Stats.PowerTokens.ConsolidatePower += tokens;
    }

    private static void CountPowerConsolidation(this HouseScore[] houseScores, LogItem logItem)
    {
        var matches = Regex.Matches(logItem.Message, @"(\w+)\(\+(\d+)\)");
        foreach (Match match in matches)
        {
            Enum.TryParse<House>(match.Groups[1].Value, out var house);
            var tokens = int.Parse(match.Groups[2].Value);
            houseScores.Get(house).Stats.PowerTokens.ConsolidatePower += tokens;
        }
    }

    private static void CountGameOfThrones(this HouseScore[] houseScores, LogItem logItem)
    {
        var matches = Regex.Matches(logItem.Message, @"(\w+)\(\+(\d+)\)");
        foreach (Match match in matches)
        {
            Enum.TryParse<House>(match.Groups[1].Value, out var house);
            var tokens = int.Parse(match.Groups[2].Value);
            houseScores.Get(house).Stats.PowerTokens.GameOfThrones += tokens;
        }
    }

    public static void CountIronThrone(this HouseScore[] houseScores, LogItem logItem)
    {
        var match = Regex.Match(logItem.Message, @"\[(\d+)\]");
        var tokens = int.Parse(match.Groups[1].Value);
        houseScores.Get(logItem.House).Stats.Bids.IronThrone += tokens;
    }

    public static void CountFiefdoms(this HouseScore[] houseScores, LogItem logItem)
    {
        var match = Regex.Match(logItem.Message, @"\[(\d+)\]");
        var tokens = int.Parse(match.Groups[1].Value);
        houseScores.Get(logItem.House).Stats.Bids.Fiefdoms += tokens;
    }

    public static void CountKingsCourt(this HouseScore[] houseScores, LogItem logItem)
    {
        var match = Regex.Match(logItem.Message, @"\[(\d+)\]");
        var tokens = int.Parse(match.Groups[1].Value);
        houseScores.Get(logItem.House).Stats.Bids.KingsCourt += tokens;
    }

    public static void CountWildlings(this HouseScore[] houseScores, LogItem logItem)
    {
        var match = Regex.Match(logItem.Message, @"\[(\d+)\]");
        var tokens = int.Parse(match.Groups[1].Value);
        houseScores.Get(logItem.House).Stats.Bids.Wildlings += tokens;
    }

    private static HouseScore Get(this IEnumerable<HouseScore> houseScores, House house) =>
        houseScores.First(score => score.House == house);
}