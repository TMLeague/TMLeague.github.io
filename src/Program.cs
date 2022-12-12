using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Caching.Memory;
using TMLeague;
using TMLeague.Http;
using TMLeague.Http.Configuration;
using TMLeague.Http.Converters;
using TMLeague.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    .Configure<LocalApiOptions>(builder.Configuration.GetSection("LocalApi"))
    .Configure<ThroneMasterApiOptions>(builder.Configuration.GetSection("ThroneMasterApi"))
    .AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
    .AddScoped<IMemoryCache, MemoryCache>()
    .AddScoped<LogConverter>()
    .AddScoped<LocalApi>()
    .AddScoped<ThroneMasterApi>()
    .AddScoped<HomeService>()
    .AddScoped<LeagueService>()
    .AddScoped<SeasonService>();

await builder.Build().RunAsync();
