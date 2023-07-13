﻿using TMApplication.ViewModels;

namespace TMApplication.Providers;

public static class RouteProvider
{
    private const string Division = "division";
    private const string Game = "game";
    private const string Index = "index";
    private const string Judge = "judge";
    private const string League = "league";
    private const string Player = "player";
    private const string Season = "season";
    private const string Seasons = "seasons";
    private const string Summary = "summary";

    public static string GetRoute(string? leagueId = null, string? seasonId = null, string? divisionId = null)
    {
        if (leagueId == null)
            return "";

        if (seasonId == null)
            return $"{League}/{leagueId}";

        if (divisionId == null)
            return $"{League}/{leagueId}/{Season}/{seasonId}";

        return $"{League}/{leagueId}/{Season}/{seasonId}/{Division}/{divisionId}";
    }

    public static string GetLeagueIndexRoute(string leagueId) =>
        $"{League}/{leagueId}/{Index}";

    public static string GetLeagueSummaryRoute(string leagueId, string? divisionId = null, TableType? tableType = null, ScoreType? scoreType = null) =>
        scoreType == null ?
            (string.IsNullOrEmpty(divisionId) ?
                $"{League}/{leagueId}/{Summary}" :
                $"{League}/{leagueId}/{Summary}/{TableType.Players}/{ScoreType.Best}/{divisionId}") :
        string.IsNullOrEmpty(divisionId)
            ? $"{League}/{leagueId}/{Summary}/{tableType}/{ScoreTypes.Get(tableType, scoreType)}"
            : $"{League}/{leagueId}/{Summary}/{tableType}/{ScoreTypes.Get(tableType, scoreType)}/{divisionId}";

    public static string GetPlayerRoute(string? leagueId, string playerName) =>
        leagueId == null ?
            GetPlayerRoute(playerName) :
            $"{League}/{leagueId}/{Player}/{playerName}";

    public static string GetPlayerRoute(string playerName) =>
            $"{Player}/{playerName}";

    public static string GetLeagueSeasonsRoute(string leagueId) =>
        $"{League}/{leagueId}/{Seasons}";

    public static string GetLeagueSeasonRoute(string leagueId, string seasonId) =>
        $"{League}/{leagueId}/{Season}/{seasonId}";

    public static string GetLeagueDivisionRoute(string leagueId, string seasonId, string divisionId) =>
        $"{League}/{leagueId}/{Season}/{seasonId}/{Division}/{divisionId}";

    public static string GetLeagueJudgeRoute(string leagueId) =>
        $"{League}/{leagueId}/{Judge}";

    public static string GetGameRoute(string? leagueId, int gameId) =>
        leagueId == null ?
            GetGameRoute(gameId) :
            $"{League}/{leagueId}/{Game}/{gameId}";

    public static string GetGameRoute(int gameId) =>
        $"{Game}/{gameId}";
}