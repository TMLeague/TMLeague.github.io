using System.Collections.Concurrent;
using TMModels;

namespace TMDraftsGenerator;

internal class DraftScoresBag
{
    private readonly ConcurrentDictionary<string, DraftScore> _draftScores = new();
    private readonly SemaphoreSlim _semaphore = new(1);
    public int Count => _draftScores.Count;
    public ICollection<DraftScore> Values => _draftScores.Values;

    public bool IsDominated(DraftScore score) => _draftScores.Values.Any(draftScore => draftScore.IsDominating(score));

    public async Task Add(DraftScore newScore, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            foreach (var (i, draftScore) in _draftScores)
                if (newScore.IsDominating(draftScore))
                    _draftScores.TryRemove(i, out _);
            _draftScores.TryAdd(newScore.Id, newScore);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}