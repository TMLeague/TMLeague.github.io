namespace TMModels;

public record Division(
    string Name,
    string Judge,
    string[] Players,
    int[] Games,
    Penalty[]? Penalties,
    Replacement[]? Replacements,
    bool IsFinished,
    string? WinnerTitle);

public record Penalty(
    string Player,
    int? Game,
    double Points,
    string Details);

public record Replacement(
    string From,
    string To,
    int Game);