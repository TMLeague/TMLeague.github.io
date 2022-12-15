namespace TMInfrastructure.Http
{
    public static class ThroneMasterUriProvider
    {
        public static string GetGameUri(uint gameId) => $"https://game.thronemaster.net/?game={gameId}";
        public static string GetPlayerUri(string playerName) => $"https://www.thronemaster.net/?goto=community&sub=members&usr={playerName}";
    }
}
