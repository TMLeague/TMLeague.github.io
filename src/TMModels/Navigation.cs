namespace TMModels;

public record Navigation(string? First, string? Previous, string? Next, string? Last)
{
    public Navigation() : this(null, null, null, null) { }
}