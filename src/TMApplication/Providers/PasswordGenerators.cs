namespace TMApplication.Providers;

public interface IPasswordGenerator
{
    IEnumerable<string> Get(int passwordLength, int count);
}

public class RandomPasswordGenerator : IPasswordGenerator
{
    private const string Characters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

    public IEnumerable<string> Get(int passwordLength, int count)
    {
        for (var i = 0; i < count; i++)
            yield return new string(Enumerable.Range(0, passwordLength)
                .Select(_ => NextChar()).ToArray());
    }

    private static char NextChar() =>
        Characters[Random.Shared.Next(Characters.Length)];
}
