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

    [Required(ErrorMessage = "Division must have at least 3 games.")]
    [CustomValidation(typeof(DivisionConfigurationForm), nameof(ValidateGames))]
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

    public static ValidationResult? ValidatePlayers(List<DivisionConfigurationFormPlayer> players, ValidationContext context)
    {
        if (players.Any(player => string.IsNullOrWhiteSpace(player.Name)))
            return new ValidationResult("Player name is required.");

        if (players.Select(player => player.Name).Distinct().Count() == players.Count)
            return new ValidationResult("Players contain duplicates.");

        return players.Count(player => !string.IsNullOrWhiteSpace(player.Name)) < 3
            ? new ValidationResult("Division must have at least 3 players.")
            : ValidationResult.Success;
    }

    public static ValidationResult? ValidateGames(IEnumerable<DivisionConfigurationFormGame> games, ValidationContext context) =>
        games.Count(game => game.TmId > 0) < 3
            ? new ValidationResult("Division must have at least 3 games.")
            : ValidationResult.Success;
}

public record DivisionConfigurationFormPlayer(int Idx, List<DivisionConfigurationFormGame> Games, string Name = "")
{
    public int Idx { get; set; } = Idx;

    [Required]
    [MinLength(1, ErrorMessage = "Thronemaster player name is required.")]
    public string Name { get; set; } = Name;
}

public record DivisionConfigurationFormGame(int Idx, int? TmId = null)
{
    public int Idx { get; set; } = Idx;
    public int? TmId { get; set; } = TmId;
}

public record DivisionConfigurationFormPenalty(int Idx, string? Player = null, int? Game = null, double Points = 0, string Details = "")
{
    public int Idx { get; set; } = Idx;

    [Required]
    [MinLength(1, ErrorMessage = "Thronemaster player name is required.")]
    public string? Player { get; set; } = Player;

    public int? Game { get; set; } = Game;

    public double Points { get; set; } = Points;

    public string? Details { get; set; } = Details;
}

public record DivisionConfigurationFormReplacement(int Idx, string? From = null, string? To = null, int? Game = null)
{
    public int Idx { get; set; } = Idx;

    [Required]
    [MinLength(1, ErrorMessage = "Replaced thronemaster player name is required.")]
    public string? From { get; set; } = From;

    [Required]
    [MinLength(1, ErrorMessage = "Replacing thronemaster player name is required.")]
    public string? To { get; set; } = To;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Thronemaster game ID of a replacement is required.")]
    public int? Game { get; set; } = Game;
}