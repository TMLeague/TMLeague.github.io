namespace TMModels;

public record Player(
    string Name,
    Dictionary<string, PlayerLeague> Leagues);

public record PlayerLeague(
    Dictionary<House, HouseGames> Houses)
{
    public void AddGame(string seasonId, string divisionId, HouseResult playerResultHouse)
    {
        if (!Houses.TryGetValue(playerResultHouse.House, out var playerLeagueHouse))
        {
            playerLeagueHouse = new HouseGames();
            Houses.Add(playerResultHouse.House, playerLeagueHouse);
        }

        var game = new PlayerGame(seasonId, divisionId, playerResultHouse);
        playerLeagueHouse.Add(game);
    }
}

public class HouseGames : List<PlayerGame> { }

public record PlayerGame(
    string SeasonId,
    string DivisionId,
    HouseResult Result);