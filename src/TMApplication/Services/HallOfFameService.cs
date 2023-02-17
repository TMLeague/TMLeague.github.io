using TMApplication.Providers;
using TMApplication.ViewModels;
using TMModels;

namespace TMApplication.Services;

public class HallOfFameService
{
    private readonly IDataProvider _dataProvider;

    public HallOfFameService(IDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    public async Task<HallOfFameViewModel?> GetHallOfFameVm(
        string leagueId, CancellationToken cancellationToken = default)
    {
        var summary = await _dataProvider.GetSummary(leagueId, cancellationToken);
        if (summary == null)
            return null;

        var sections = new List<HallOfFameSectionViewModel>
        {
            GetHallOfFameSectionVm(summary, "Most points in a single season", score => score.TotalPoints),
            GetHallOfFameSectionVm(summary, "Most wins in a single season", score => score.Wins),
            GetHallOfFameSectionVm(summary, "Most land territories in a single season", score => score.Cla),
            GetHallOfFameSectionVm(summary, "Most supplies in a single season", score => score.Supplies)
        };

        return new HallOfFameViewModel(sections);
    }

    private static HallOfFameSectionViewModel GetHallOfFameSectionVm(
        Summary summary, string title, Func<SummaryScore, double> getValue)
    {
        var divisions = summary.Divisions
            .Select(division => GetHallOfFameDivisionVm(summary.LeagueId, division, getValue))
            .ToArray();

        return new HallOfFameSectionViewModel(title, divisions);
    }

    private static HallOfFameDivisionViewModel GetHallOfFameDivisionVm(
        string leagueId, SummaryDivision division, Func<SummaryScore, double> getValue) => new(
        leagueId,
        division.DivisionId,
        division.DivisionName,
        division.Players
            .Select(score => new HallOfFamePlayerViewModel(
                score.Player,
                getValue(score.Best)))
            .OrderByDescending(model => model.Score)
            .Take(5)
            .ToArray());
}