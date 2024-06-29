namespace TMModels;

public record Player(
    string Name,
    Dictionary<string, PlayerLeague> Leagues)
{
    public Player() : this(string.Empty, [])
    {
        GeneratedTime = DateTimeOffset.UtcNow;
    }

    public Player(string name, DateTimeOffset generatedTime) : this(name, [])
    {
        GeneratedTime = generatedTime;
    }

    public DateTimeOffset GeneratedTime { get; set; }
}

public record PlayerLeague(string LeagueId, List<PlayerDivision> Results)
{
    public void AddDivision(string seasonId, string divisionId, PlayerResult playerResult) =>
        Results.Add(new PlayerDivision(seasonId, divisionId, playerResult));
}

public record PlayerDivision(
    string SeasonId,
    string DivisionId,
    PlayerResult Result);