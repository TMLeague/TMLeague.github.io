using System.Text.RegularExpressions;
using TMModels;
using TMModels.ThroneMaster;

namespace TMGameImporter.Http.Converters;

internal static class StatsConverter
{
    public static HouseScore[] CalculateStats(this HouseScore[] houseScores, Log log)
    {
        Battle? battle = null;
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
                    if (logItem.Message.Contains("[Attack]"))
                        battle = GetBattle(logItem);
                    break;
                case Phase.Battle:
                    if (battle != null && logItem.Message.Contains("supports") && logItem.Message.Contains("with a fighting strength of"))
                        battle.AddSupport(logItem);
                    if (battle != null && logItem.Message.Contains("to lead his forces."))
                        battle.UpdateFightingHouse(logItem);
                    if (battle != null && logItem.Message.Contains("used Aeron's special ability to choose another house card."))
                        battle.IsAeronUsed = true;
                    if (battle != null && logItem.Message.Contains("lost the battle"))
                    {
                        battle.UpdateOutcome(logItem);
                        houseScores.CountBattle(battle);
                    }
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
                    if (logItem.Message.Contains("Power tokens to his available Power."))
                        houseScores.CountWildlingsSkinchanger(logItem);
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

    private static Battle? GetBattle(LogItem logItem)
    {
        var match = Regex.Match(logItem.Message, @"(\w+) is attacking (\w+) in (.+) from (.+) with (((?<Footmen>\d Footm\wn)|(?<Knights>\d Knight)|(?<SiegeEngines>\d Siege Engine)|(?<Ships>\d Ship))(( and )|(, ))?)+");
        if (!match.Success)
            return null;

        _ = Enum.TryParse<House>(match.Groups[2].Value, out var defender);
        var area = match.Groups[3].Value;
        var from = match.Groups[4].Value;
        var footmen = GetUnitsFromGroup(match, "Footmen");
        var knights = GetUnitsFromGroup(match, "Knights");
        var siegeEngines = GetUnitsFromGroup(match, "SiegeEngines");
        var ships = GetUnitsFromGroup(match, "Ships");
        var attackerUnits = new UnitStats(footmen, knights, siegeEngines, ships);

        return new Battle(new FightingHouse(logItem.House), new FightingHouse(defender), area, from, attackerUnits);

        static int GetUnitsFromGroup(Match match, string name) =>
            string.IsNullOrEmpty(match.Groups[name].Value) ? 0 : int.Parse(match.Groups[name].Value[..1]);
    }

    private static void CountBattle(this HouseScore[] houseScores, Battle battle)
    {
        if (battle.Winner == null || battle.Looser == null)
            return;

        var winnerScore = houseScores.Get(battle.Winner.House);
        var looserScore = houseScores.Get(battle.Looser.House);

        if (winnerScore.House == battle.Attacker!.House)
        {
            winnerScore.Stats.AddSuccessfulAttack(looserScore.House, battle.Winner.Casualties, battle.Looser.Casualties, battle.AttackerSupporters, battle.DefenderSupporters);
            looserScore.Stats.AddLostDefenses(winnerScore.House, battle.Winner.Casualties, battle.Looser.Casualties, battle.AttackerSupporters, battle.DefenderSupporters);
        }
        else
        {
            winnerScore.Stats.AddSuccessfulDefense(looserScore.House, battle.Winner.Casualties, battle.Looser.Casualties, battle.AttackerSupporters, battle.DefenderSupporters);
            looserScore.Stats.AddLostAttack(winnerScore.House, battle.Winner.Casualties, battle.Looser.Casualties, battle.AttackerSupporters, battle.DefenderSupporters);
        }

        foreach (var (supporter, _) in battle.AttackerSupporters)
            houseScores.Get(supporter).Stats.AddSupport(battle.Attacker.House, battle.Defender!.House);
        foreach (var (supporter, _) in battle.DefenderSupporters)
            houseScores.Get(supporter).Stats.AddSupport(battle.Defender!.House, battle.Attacker.House);

        if (battle.IsAeronUsed)
            houseScores.Get(House.Greyjoy).Stats.Bids.Aeron += 2;

        if (battle.Winner.House == House.Lannister && battle.Winner.Card?.Name == HouseCard.Tywin)
            houseScores.Get(House.Lannister).Stats.PowerTokens.Tywin += 2;
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

    private static void CountIronThrone(this HouseScore[] houseScores, LogItem logItem)
    {
        var match = Regex.Match(logItem.Message, @"\[(\d+)\]");
        var tokens = int.Parse(match.Groups[1].Value);
        houseScores.Get(logItem.House).Stats.Bids.IronThrone += tokens;
    }

    private static void CountFiefdoms(this HouseScore[] houseScores, LogItem logItem)
    {
        var match = Regex.Match(logItem.Message, @"\[(\d+)\]");
        var tokens = int.Parse(match.Groups[1].Value);
        houseScores.Get(logItem.House).Stats.Bids.Fiefdoms += tokens;
    }

    private static void CountKingsCourt(this HouseScore[] houseScores, LogItem logItem)
    {
        var match = Regex.Match(logItem.Message, @"\[(\d+)\]");
        var tokens = int.Parse(match.Groups[1].Value);
        houseScores.Get(logItem.House).Stats.Bids.KingsCourt += tokens;
    }

    private static void CountWildlings(this HouseScore[] houseScores, LogItem logItem)
    {
        var match = Regex.Match(logItem.Message, @"\[(\d+)\]");
        var tokens = int.Parse(match.Groups[1].Value);
        houseScores.Get(logItem.House).Stats.Bids.Wildlings += tokens;
    }

    private static void CountWildlingsSkinchanger(this HouseScore[] houseScores, LogItem logItem)
    {
        var match = Regex.Match(logItem.Message, @"(\w+) returned \[(\d+)\] Power tokens to his available Power");
        if (!match.Success)
            return;

        _ = Enum.TryParse<House>(match.Groups[1].Value, out var house);
        var tokens = int.Parse(match.Groups[2].Value);
        houseScores.Get(house).Stats.PowerTokens.Wildlings += tokens;
    }

    private static HouseScore Get(this IEnumerable<HouseScore> houseScores, House house) =>
        houseScores.First(score => score.House == house);

    private record Battle(FightingHouse Attacker, FightingHouse Defender, string Area, string FromArea, UnitStats AttackerUnits)
    {
        public FightingHouse? Attacker { get; set; } = Attacker;
        public FightingHouse? Defender { get; set; } = Defender;
        public FightingHouse? Winner { get; set; }
        public FightingHouse? Looser => Winner == null ?
            null :
            Winner == Attacker ?
                Defender : Attacker;

        public Dictionary<House, int> AttackerSupporters { get; } = new();
        public Dictionary<House, int> DefenderSupporters { get; } = new();

        public bool IsAeronUsed { get; set; }

        public void UpdateFightingHouse(LogItem logItem)
        {
            var match = Regex.Match(logItem.Message, @"(\w+) chose \((\d)\) ([\w ]+) to lead his forces\.");
            if (!match.Success)
                return;

            var strength = int.Parse(match.Groups[2].Value);
            var houseCardName = match.Groups[3].Value;
            if (Attacker?.House == logItem.House)
                Attacker.Card = new HouseCard(strength, houseCardName);
            else if (Defender != null)
                Defender.Card = new HouseCard(strength, houseCardName);
        }

        public void UpdateOutcome(LogItem logItem)
        {
            Winner = Attacker?.House == logItem.House ?
                Defender : Attacker;

            var footmenMatch = Regex.Match(logItem.Message, @"(\d+) Footm\wn");
            if (footmenMatch.Success && int.TryParse(footmenMatch.Groups[1].Value, out var footmen))
                Looser!.Casualties.Footmen = footmen;

            var knightsMatch = Regex.Match(logItem.Message, @"(\d+) Knight");
            if (knightsMatch.Success && int.TryParse(knightsMatch.Groups[1].Value, out var knights))
                Looser!.Casualties.Knights = knights;

            var shipsMatch = Regex.Match(logItem.Message, @"(\d+) Ship");
            if (shipsMatch.Success && int.TryParse(shipsMatch.Groups[1].Value, out var ships))
                Looser!.Casualties.Ships = ships;

            var siegeEnginesMatch = Regex.Match(logItem.Message, @"(\d+) Siege");
            if (siegeEnginesMatch.Success && int.TryParse(siegeEnginesMatch.Groups[1].Value, out var siegeEngines))
                Looser!.Casualties.SiegeEngines = siegeEngines;

            if (Looser!.Card?.Name == HouseCard.Mace && Looser.Casualties.Footmen > 0 &&
                (Looser == Defender && AttackerUnits.Footmen > 0 || Looser == Attacker && AttackerUnits.Footmen == 0))
            {
                Looser.Casualties.Footmen--;
                Winner!.Casualties.Footmen++;
            }
        }

        public void AddSupport(LogItem logItem)
        {
            var supportMatch = Regex.Match(logItem.Message, @"(\w+) supports (\w+) with a fighting strength of (\d+)\.");
            if (!supportMatch.Success || !int.TryParse(supportMatch.Groups[3].Value, out var support)) 
                return;

            var supportedHouse = Enum.Parse<House>(supportMatch.Groups[2].Value);
            if (Attacker?.House == supportedHouse) 
                AttackerSupporters[logItem.House] = support;
            else if (Defender?.House == supportedHouse)
                DefenderSupporters[logItem.House] = support;
        }
    }

    private record FightingHouse(House House)
    {
        public readonly UnitStats Casualties = new();
        public HouseCard? Card { get; set; }
    }

    private record HouseCard(int Strength, string Name)
    {
        public const string Tywin = "Tywin Lannister";
        public const string Mace = "Mace Tyrell";
    }
}