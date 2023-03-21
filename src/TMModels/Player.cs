namespace TMModels;

public record Player(
    string Name,
    PlayerSeason[] Seasons,
    PlayerHouse[] Houses);

public record PlayerSeason(
    string SeasonId,
    string DivisionId,
    PlayerGame[] Games);

public record PlayerGame(
    string GameId,
    string House);

public record PlayerHouse(
    string SeasonId,
    string DivisionId,
    string House);