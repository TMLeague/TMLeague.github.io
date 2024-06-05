using System.ComponentModel.DataAnnotations;

namespace TMModels;

public class DivisionConfigurationForm
{
    public DivisionConfigurationForm() { }
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
    public List<DivisionConfigurationFormPlayer> Players { get; } = new();

    [Required(ErrorMessage = "Division must have at least 3 games.")]
    [CustomValidation(typeof(DivisionConfigurationForm), nameof(ValidateGames))]
    public List<DivisionConfigurationFormGame> Games { get; } = new();

    [CustomValidation(typeof(DivisionConfigurationForm), nameof(ValidatePenalties))]
    public List<DivisionConfigurationFormPenalty> Penalties { get; } = new();

    [CustomValidation(typeof(DivisionConfigurationForm), nameof(ValidateReplacements))]
    public List<DivisionConfigurationFormReplacement> Replacements { get; } = new();

    public bool IsFinished { get; set; }

    public string? WinnerTitle { get; set; }

    public int? Promotions { get; set; }

    public int? Relegations { get; set; }

    public bool CanModifyLists => Penalties.Count == 0 && Replacements.Count == 0 && !IsFinished;

    public IEnumerable<DivisionConfigurationFormGame> GetPlayerGames(string? player) =>
        string.IsNullOrWhiteSpace(player) ? Games : Players
            .FirstOrDefault(p => p.Name == player)?.Games ?? Games;

    public static ValidationResult? ValidatePlayers(IList<DivisionConfigurationFormPlayer> players, ValidationContext context)
    {
        if (players.Any(player => string.IsNullOrWhiteSpace(player.Name)))
            return new ValidationResult("Division player name is required.");

        if (players.Select(player => player.Name).Distinct().Count() < players.Count)
            return new ValidationResult("Division players contain duplicates.");

        return players.Count(player => !string.IsNullOrWhiteSpace(player.Name)) < 3
            ? new ValidationResult("Division must have at least 3 players.")
            : ValidationResult.Success;
    }

    public static ValidationResult? ValidateGames(IList<DivisionConfigurationFormGame> games, ValidationContext context)
    {
        var notNullGames = games.Where(game => game.TmId != null).ToArray();
        if (notNullGames.Select(game => game.TmId).Distinct().Count() < notNullGames.Length)
            return new ValidationResult("Division games contain duplicates.");

        return games.Count < 3
            ? new ValidationResult("Division must have at least 3 games.")
            : ValidationResult.Success;
    }

    public static ValidationResult? ValidatePenalties(IEnumerable<DivisionConfigurationFormPenalty> penalties, ValidationContext context)
    {
        var form = (DivisionConfigurationForm)context.ObjectInstance;
        foreach (var penalty in penalties)
        {
            if (string.IsNullOrWhiteSpace(penalty.Player))
                return new ValidationResult("Penalty player name is required.");

            if (form.Players.All(player => player.Name != penalty.Player))
                return new ValidationResult($"Penalty player {penalty.Player} not exists on players list.");

            if (penalty.Game != null && form.Games.All(game => game.TmId != penalty.Game))
                return new ValidationResult($"Penalty game {penalty.Game} not exists on games list.");
        }

        return ValidationResult.Success;
    }

    public static ValidationResult? ValidateReplacements(IEnumerable<DivisionConfigurationFormReplacement> replacements, ValidationContext context)
    {
        var form = (DivisionConfigurationForm)context.ObjectInstance;
        foreach (var replacement in replacements)
        {
            if (string.IsNullOrWhiteSpace(replacement.From))
                return new ValidationResult("Replaced player name (from) is required.");

            if (form.Players.All(player => player.Name != replacement.From))
                return new ValidationResult($"Replaced player {replacement.From} not exists on players list.");

            if (string.IsNullOrWhiteSpace(replacement.To))
                return new ValidationResult("Replacing player name (to) is required.");

            if (replacement.Game is null or < 1)
                return new ValidationResult("Replacement game is required.");

            if (replacement.Game != null && form.Games.All(game => game.TmId != replacement.Game))
                return new ValidationResult($"Replacement game {replacement.Game} not exists on games list.");
        }

        return ValidationResult.Success;
    }
}

public record DivisionConfigurationFormPlayer(int Idx, IList<DivisionConfigurationFormGame> Games, string Name = "")
{
    public int Idx { get; set; } = Idx;

    [Required(ErrorMessage = "Thronemaster player name is required.")]
    [MinLength(1, ErrorMessage = "Thronemaster player name is required.")]
    public string Name { get; set; } = Name;
}

public record DivisionConfigurationFormGame(int Idx, int? TmId = null)
{
    public int Idx { get; set; } = Idx;
    public int? TmId { get; set; } = TmId;
}

public record DivisionConfigurationFormPenalty(int Idx, string? Player = null, int? Game = null, double Points = 0, string Details = "", bool Disqualification = false)
{
    public int Idx { get; set; } = Idx;

    [Required(ErrorMessage = "Penalty thronemaster player name is required.")]
    [MinLength(1, ErrorMessage = "Penalty thronemaster player name is required.")]
    public string? Player { get; set; } = Player;

    public int? Game { get; set; } = Game;

    public double Points { get; set; } = Points;

    public string? Details { get; set; } = Details;

    public bool Disqualification { get; set; } = Disqualification;
}

public record DivisionConfigurationFormReplacement(int Idx, string? From = null, string? To = null, int? Game = null)
{
    public int Idx { get; set; } = Idx;

    [Required(ErrorMessage = "Replaced thronemaster player name is required.")]
    [MinLength(1, ErrorMessage = "Replaced thronemaster player name is required.")]
    public string? From { get; set; } = From;

    [Required(ErrorMessage = "Replacing thronemaster player name is required.")]
    [MinLength(1, ErrorMessage = "Replacing thronemaster player name is required.")]
    public string? To { get; set; } = To;

    [Required(ErrorMessage = "Thronemaster game ID of a replacement is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Thronemaster game ID of a replacement is required.")]
    public int? Game { get; set; } = Game;
}