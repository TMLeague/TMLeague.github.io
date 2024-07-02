using TMModels.ThroneMaster;

namespace TMMovePrediction;

public record Action(string Player, Phase phase, DateTime timestamp);
