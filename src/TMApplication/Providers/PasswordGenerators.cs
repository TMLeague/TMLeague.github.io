using TMApplication.Extensions;

namespace TMApplication.Providers;

public interface IPasswordGenerator
{
    Task<IEnumerable<string>> Get(int passwordLength, int count, CancellationToken cancellationToken);
}

public class RandomPasswordGenerator : IPasswordGenerator
{
    private const string Characters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

    public Task<IEnumerable<string>> Get(int passwordLength, int count, CancellationToken cancellationToken) =>
        Task.FromResult(Get(passwordLength, count));

    private static IEnumerable<string> Get(int passwordLength, int count)
    {
        for (var i = 0; i < count; i++)
            yield return new string(Enumerable.Range(0, passwordLength)
                .Select(_ => NextChar()).ToArray());
    }

    private static char NextChar() =>
        Characters[Random.Shared.Next(Characters.Length)];
}

public class GameOfThronePasswordGenerator : IPasswordGenerator
{
    private readonly IDataProvider _provider;
    private string[]? _passwords;

    public GameOfThronePasswordGenerator(IDataProvider provider)
    {
        _provider = provider;
    }
    public async Task<IEnumerable<string>> Get(int passwordLength, int count, CancellationToken cancellationToken)
    {
        _passwords ??= await _provider.GetPasswords(CancellationToken.None) ?? Array.Empty<string>();

        return _passwords.Length switch
        {
            0 => await new RandomPasswordGenerator().Get(passwordLength, count, cancellationToken), // Random
            var n when n >= count => _passwords.Shuffle().Take(count), // Sampling Without Replacement
            _ => GetRandom(_passwords, count) // Draws with Replacement
        };
    }

    private static IEnumerable<T> GetRandom<T>(IReadOnlyList<T> array, int count)
    {
        for (var i = 0; i < count; i++)
            yield return array[Random.Shared.Next(array.Count)];
    }
}
