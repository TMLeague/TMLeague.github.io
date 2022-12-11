using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Caching.Memory;
using TMLeague;
using TMLeague.Http;
using TMLeague.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
    .AddScoped<HomeService>()
    .AddScoped<LeagueService>()
    .AddScoped<SeasonService>()
    .AddScoped<LocalApi>()
    .AddScoped<ThroneMasterApi>()
    .AddSingleton<IMemoryCache, MemoryCache>();

await builder.Build().RunAsync();
