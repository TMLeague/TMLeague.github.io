using TMModels;

namespace TMGameImporter.Http.Converters.Models;

internal record State(
    DateTimeOffset Time,
    string? Chat,
    uint? GameId,
    bool IsFinished,
    uint Turn,
    string[][] MapDefinition,
    HouseSpeed[] Stats);

internal record HouseSpeed(House House, double MinutesPerMove, uint MovesCount);