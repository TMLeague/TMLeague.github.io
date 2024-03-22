using System.ComponentModel.DataAnnotations;

namespace TMModels;

public class DivisionConfigurationForm
{
    public DivisionConfigurationForm(string? name, string? judge, List<DivisionConfigurationFormPlayer>? players, List<DivisionConfigurationFormGame>? games, List<DivisionConfigurationFormPenalty>? penalties, List<DivisionConfigurationFormReplacement>? replacements, bool isFinished, string? winnerTitle, int? promotions, int? relegations)
    {
        Name = name;
        Judge = judge;
        Players = players ?? new List<DivisionConfigurationFormPlayer>();
        Games = games ?? new List<DivisionConfigurationFormGame>();
        Penalties = penalties ?? new List<DivisionConfigurationFormPenalty>();
        Replacements = replacements ?? new List<DivisionConfigurationFormReplacement>();
        IsFinished = isFinished;
        WinnerTitle = winnerTitle;
        Promotions = promotions;
        Relegations = relegations;
    }

    [Required(ErrorMessage = "Division must have name.")]
    [MinLength(1, ErrorMessage = "Division must have name.")]
    public string? Name { get; set; }

    public string? Judge { get; set; }

    [Required(ErrorMessage = "Division must have at least 3 players.")]
    [CustomValidation(typeof(DivisionConfigurationForm), nameof(ValidatePlayers))]
    public List<DivisionConfigurationFormPlayer> Players { get; set; }

    public List<DivisionConfigurationFormGame> Games { get; set; }

    public List<DivisionConfigurationFormPenalty> Penalties { get; set; }

    public List<DivisionConfigurationFormReplacement> Replacements { get; set; }

    public bool IsFinished { get; set; }

    public string? WinnerTitle { get; set; }

    public int? Promotions { get; set; }

    public int? Relegations { get; set; }

    public IEnumerable<DivisionConfigurationFormGame> GetPlayerGames(string? player) =>
        string.IsNullOrWhiteSpace(player) ? Games : Players
            .FirstOrDefault(p => p.Name == player)?.Games ?? Games;

    public static ValidationResult? ValidatePlayers(IEnumerable<DivisionConfigurationFormPlayer> players, ValidationContext context) =>
        players.Count(player => !string.IsNullOrWhiteSpace(player.Name)) < 3
            ? new ValidationResult("Division must have at least 3 players.")
            : ValidationResult.Success;
}

public record DivisionConfigurationFormPlayer(int Idx, List<DivisionConfigurationFormGame> Games)
{
    public string Name { get; set; } = string.Empty;
}

public record DivisionConfigurationFormGame(int Idx)
{
    public DivisionConfigurationFormGame(int? tmId, int idx) : this(idx)
    {
        TmId = tmId;
    }

    public int? TmId { get; set; }
}

public record DivisionConfigurationFormPenalty(int Idx)
{
    public DivisionConfigurationFormPenalty(int idx, string player, int? game, double points, string details) : this(idx)
    {
        Player = player;
        Game = game;
        Points = points;
        Details = details;
    }

    public string? Player { get; set; }
    public int? Game { get; set; }
    public double Points { get; set; }
    public string? Details { get; set; }
}

public record DivisionConfigurationFormReplacement(int Idx)
{
    public DivisionConfigurationFormReplacement(int idx, string from, string to, int game) : this(idx)
    {
        From = from;
        To = to;
        Game = game;
    }

    public string? From { get; set; }
    public string? To { get; set; }
    public int? Game { get; set; }
}