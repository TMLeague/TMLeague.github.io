using System.Text.Json.Serialization;

namespace TMModels;

public record Player(
    string Name,
    int RankPoints,
    string Country, 
    string Location, 
    string Speed);