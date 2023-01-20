using FluentAssertions;
using TMApplication.Services;

namespace TMApplication.Tests;

public class DraftServiceTests
{
    private readonly DraftService _sut;

    public DraftServiceTests()
    {
        _sut = new DraftService();
    }

    [Fact]
    public async Task GetDraft_Should_GenerateDraft()
    {
        var draft = await _sut.GetDraft(3, 3);
        draft.Should().NotBeNull();
    }
}