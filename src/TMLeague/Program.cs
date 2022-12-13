using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Caching.Memory;
using TMApplication.Providers;
using TMApplication.Services;
using TMInfrastructure.Http;
using TMInfrastructure.Http.Configuration;
using TMInfrastructure.Http.Converters;
using TMLeague;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    .AddCors(options =>
    {
        options.AddDefaultPolicy(policyBuilder =>
            policyBuilder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
    })
    .Configure<LocalApiOptions>(builder.Configuration.GetSection("LocalApi"))
    .Configure<ThroneMasterApiOptions>(builder.Configuration.GetSection("ThroneMasterApi"))
    .AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
    .AddScoped<IMemoryCache, MemoryCache>()
    .AddScoped<LogConverter>()
    .AddScoped<IDataProvider, LocalApi>()
    .AddScoped<HomeService>()
    .AddScoped<LeagueService>()
    .AddScoped<SeasonService>()
    .AddScoped<DivisionService>()
    .AddScoped<GameService>()
    .AddScoped<PlayerService>();

await builder.Build().RunAsync();
