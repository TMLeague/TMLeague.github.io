namespace TMInfrastructure.Http.Configuration;

public class LocalApiOptions
{
    public CacheOptions Cache { get; init; } = new(TimeSpan.FromHours(1), TimeSpan.FromHours(1));
}
public class ThroneMasterApiOptions
{
    public CacheOptions Cache { get; init; } = new(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
}

public class CacheOptions
{
    public TimeSpan DefaultExpirationTime { get; init; } = TimeSpan.FromMinutes(1);
    public TimeSpan NotFoundExpirationTime { get; init; } = TimeSpan.FromMinutes(1);

    public CacheOptions() { }

    public CacheOptions(TimeSpan defaultExpirationTime, TimeSpan notFoundExpirationTime)
    {
        DefaultExpirationTime = defaultExpirationTime;
        NotFoundExpirationTime = notFoundExpirationTime;
    }
}