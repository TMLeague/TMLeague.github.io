using TMModels.ThroneMaster;
using Action = TMModels.Action;

namespace TMTools.Converters;

public class ActionsConverter
{
    public List<Action> Convert(Log log) =>
        log.Logs
            .Where(item => item.Date.HasValue && !string.IsNullOrEmpty(item.Player))
            .Select(item => new Action(item.Player, item.Phase, item.Date!.Value.DateTime)).ToList();
}