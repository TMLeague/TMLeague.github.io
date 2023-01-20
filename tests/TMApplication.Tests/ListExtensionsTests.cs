using FluentAssertions;
using TMApplication.Extensions;

namespace TMApplication.Tests;

public class ListExtensionsTests
{
    [Fact]
    public void Shuffle_Should_Returns_AllItems()
    {
        var list = Enumerable.Range(0, 10).ToList();
        var result = list.ToList().Shuffle();
        result.Should().Contain(list);
    }

    [Fact]
    public void Distinct_Should_NotChangeOrder()
    {
        var list = Enumerable.Range(0, 7)
            .Concat(Enumerable.Repeat(0, 3))
            .ToList().Shuffle();
        var result = list.Distinct().ToList();
        list.Should().ContainInOrder(result);
    }
}