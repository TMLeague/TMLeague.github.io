using FakeItEasy;
using FluentAssertions;
using TMApplication.Providers;

namespace TMApplication.Tests;
public class GameOfThronePasswordGeneratorTests
{
    private readonly IPasswordGenerator _generator;
    private readonly IDataProvider _provider;

    public GameOfThronePasswordGeneratorTests()
    {
        _provider = A.Fake<IDataProvider>();
        _generator = new GameOfThronePasswordGenerator(_provider);
    }

    [Fact]
    public async Task Should_ReturnRandomPasswords_When_0()
    {
        // arrange
        const int passwordLength = 7;
        const int count = 3;
        A.CallTo(() => _provider.GetPasswords(A<CancellationToken>._))!
            .Returns(Task.FromResult(Array.Empty<string>()));

        // act
        var passwords = (await _generator.Get(passwordLength, count, CancellationToken.None)).ToArray();

        // assert
        passwords.Should().HaveCount(count);
        foreach (var password in passwords)
            password.Should().HaveLength(passwordLength);
    }

    [Theory]
    [InlineData(9)]
    [InlineData(10)]
    public async Task Should_ReturnRandomPasswordsWithoutReplacement_When_CountLessOrEqualArrayLength(int count)
    {
        // arrange
        const int passwordLength = 7;
        A.CallTo(() => _provider.GetPasswords(A<CancellationToken>._))!
            .Returns(Task.FromResult(new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" }));

        // act
        var passwords = (await _generator.Get(passwordLength, count, CancellationToken.None)).ToArray();

        // assert
        passwords.Should().HaveCount(count);
        foreach (var password in passwords)
            passwords.Count(p => p == password).Should().Be(1);
    }

    [Fact]
    public async Task Should_ReturnRandomPasswordsWithReplacement_When_CountGreaterThanArrayLength()
    {
        // arrange
        const int passwordLength = 7;
        const int count = 20;
        A.CallTo(() => _provider.GetPasswords(A<CancellationToken>._))!
            .Returns(Task.FromResult(new[] { "A", "B", "C" }));

        // act
        var passwords = (await _generator.Get(passwordLength, count, CancellationToken.None)).ToArray();

        // assert
        passwords.Should().HaveCount(count);
        foreach (var password in passwords)
            password.Should().HaveLength(1);
    }
}
