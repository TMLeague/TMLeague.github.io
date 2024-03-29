﻿namespace TMInfrastructure.Http;

public static class ThroneMasterUriProvider
{
    public static string GetGameUri(int gameId) =>
        $"https://game.thronemaster.net/?game={gameId}";

    public static string GetPlayerUri(string playerName) => 
        $"https://www.thronemaster.net/?goto=community&sub=members&usr={playerName}";

    public static string GetMessageUri(string? playerName) =>
        $"https://www.thronemaster.net/?goto=account&sub=private_messages&type=new&to={playerName}";
}