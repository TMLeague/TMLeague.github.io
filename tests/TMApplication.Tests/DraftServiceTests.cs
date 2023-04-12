using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TMApplication.Services;

namespace TMApplication.Tests;

public class DraftServiceTests
{
    private readonly DraftService _sut;

    public DraftServiceTests()
    {
        _sut = new DraftService(A.Fake<ILogger<DraftService>>());
    }

    [Fact]
    public void GetDraft_Should_GenerateDraft()
    {
        var draft = _sut.GetDraft(3, 3);
        draft.Should().NotBeNull();
    }
}