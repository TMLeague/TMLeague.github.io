namespace TMModels;

public record Division(
    string Name,
    string? Judge,
    string[] Players,
    int?[] Games,
    Penalty[]? Penalties,
    Replacement[]? Replacements,
    bool IsFinished,
    string? WinnerTitle,
    int? Promotions,
    int? Relegations);

public record Penalty(
    string Player,
    int? Game,
    double Points,
    string Details)
{
    public const string BattlePenalty = "for not enough battles before 10th round";
};

public record Replacement(
    string From,
    string To,
    int Game);