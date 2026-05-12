using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using TMGameImporter.Configuration;

namespace TMGameImporter.Http.Cloudflare;

public interface IHttpClient
{
    Task<string> GetAsync(string url, CancellationToken cancellationToken);
}

internal class PlaywrightClient(HttpClient client, IOptions<ImporterOptions> options, ILogger<PlaywrightClient> logger) : IHttpClient, IDisposable
{
    private Lazy<PlaywrightDisposable> _playwright = new(() => GetPage(options).GetAwaiter().GetResult());

    public async Task<string> GetAsync(string url, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(options.Value.CfClearance))
            return await client.GetStringAsync(url, cancellationToken);

        logger.LogInformation("Get content from {url} by playwight {id}", url, _playwright.Value.Id);

        var page = _playwright.Value.Page;

        await page.GotoAsync(client.BaseAddress != null ? new Uri(client.BaseAddress, url).ToString() : url);

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        return await page.ContentAsync();
    }

    private static async Task<PlaywrightDisposable> GetPage(IOptions<ImporterOptions> options)
    {
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();
        await page.SetExtraHTTPHeadersAsync(new Dictionary<string, string>
        { { "User-Agent", options.Value.UserAgent } });

        return new PlaywrightDisposable(Guid.NewGuid(), playwright, page);
    }

    public void Dispose()
    {
        if (_playwright.IsValueCreated)
            _playwright.Value.Dispose();
    }

    private record PlaywrightDisposable(Guid Id, IPlaywright Playwright, IPage Page) : IDisposable
    {
        public void Dispose() => Playwright.Dispose();
    }
}
