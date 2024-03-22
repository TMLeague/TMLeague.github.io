﻿namespace TMModels;

public class PenaltiesForm
{
    public PenaltiesForm(string name, string judge, List<PenaltyFormPlayer> players, List<PenaltyFormGame> games, List<PenaltyFormPenalty> penalties, List<PenaltyFormReplacement> replacements, bool isFinished, string? winnerTitle, int? promotions, int? relegations)
    {
        Name = name;
        Judge = judge;
        Players = players;
        Games = games;
        Penalties = penalties;
        Replacements = replacements;
        IsFinished = isFinished;
        WinnerTitle = winnerTitle;
        Promotions = promotions;
        Relegations = relegations;
    }

    public string Name { get; set; }
    public string Judge { get; set; }
    public List<PenaltyFormPlayer> Players { get; set; }
    public List<PenaltyFormGame> Games { get; set; }
    public List<PenaltyFormPenalty> Penalties { get; set; }
    public List<PenaltyFormReplacement> Replacements { get; set; }
    public bool IsFinished { get; set; }
    public string? WinnerTitle { get; set; }
    public int? Promotions { get; set; }
    public int? Relegations { get; set; }

    public IEnumerable<PenaltyFormGame> GetPlayerGames(string? player) =>
        string.IsNullOrWhiteSpace(player) ? Games : Players
            .FirstOrDefault(p => p.Name == player)?.Games ?? Games;
}

public record PenaltyFormPlayer(int Idx, List<PenaltyFormGame> Games)
{
    public string Name { get; set; } = string.Empty;
}

public record PenaltyFormGame(int Idx)
{
    public PenaltyFormGame(int? tmId, int idx) : this(idx)
    {
        TmId = tmId;
    }

    public int? TmId { get; set; }
}

public record PenaltyFormPenalty(int Idx)
{
    public PenaltyFormPenalty(int idx, string player, int? game, double points, string details) : this(idx)
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

public record PenaltyFormReplacement(int Idx)
{
    public PenaltyFormReplacement(int idx, string from, string to, int game) : this(idx)
    {
        From = from;
        To = to;
        Game = game;
    }

    public string? From { get; set; }
    public string? To { get; set; }
    public int? Game { get; set; }
}