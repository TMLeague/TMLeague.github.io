using System.Text.Json.Serialization;

namespace TMModels;

public record Draft(
    string Name,
    string[][] Table);