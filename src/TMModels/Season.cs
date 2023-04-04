using TMModels.Extensions;

namespace TMModels;

public record Season(
    string Name,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate,
    string[] Divisions
)
{
    public Navigation GetDivisionNavigation(string divisionId)
    {
        var idx = Divisions.GetIdx(divisionId);
        var first = idx > 0 ? Divisions.First() : null;
        var previous = idx > 1 ? Divisions[idx - 1] : null;
        var next = idx < Divisions.Length - 2 ? Divisions[idx + 1] : null;
        var last = idx < Divisions.Length - 1 ? Divisions.Last() : null;
        return new Navigation(first, previous, next, last);
    }
}