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
    private Lazy<PlaywrightDisposable> _playwright = new(() => GetPage(client, options, logger).GetAwaiter().GetResult());

    public async Task<string> GetAsync(string url, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(options.Value.CfClearance))
            return await client.GetStringAsync(url, cancellationToken);

        logger.LogInformation("Get content from {url} by playwight {id}", url, _playwright.Value.Id);

        var page = _playwright.Value.Page;

        var finalUrl = client.BaseAddress != null ? new Uri(client.BaseAddress, url).ToString() : url;
        await page.GotoAsync(finalUrl, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        try
        {
            await page.WaitForFunctionAsync("() => document.body.innerText.length > 0 && !document.body.innerText.includes('security verification')",
            new PageWaitForFunctionOptions { Timeout = 10000 });
        }
        catch (TimeoutException)
        {
            logger.LogWarning("A timeout occurred loading a page {url}", finalUrl);
        }

        return await page.ContentAsync();
    }

    private static async Task<PlaywrightDisposable> GetPage(HttpClient client, IOptions<ImporterOptions> options, ILogger<PlaywrightClient> logger)
    {
        var id = Guid.NewGuid();
        logger.LogInformation("Initializing Playwright {id}...", id);

        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Args = new[] {
                "--disable-blink-features=AutomationControlled",
                "--no-sandbox",
                "--disable-infobars",
                "--disable-dev-shm-usage",
                "--disable-browser-side-navigation",
                "--disable-gpu",
                "--window-size=1920,1080",
                "--user-agent=" + options.Value.UserAgent
            }
        });

        var page = await browser.NewPageAsync();
        await page.SetExtraHTTPHeadersAsync(new Dictionary<string, string>
        {
            { "Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8" },
            { "Accept-Language", "pl-PL,pl;q=0.9,en-US;q=0.8,en;q=0.7" },
            { "Cache-Control", "no-cache" },
            { "Pragma", "no-cache" },
            { "User-Agent", options.Value.UserAgent }
        });

        await page.AddInitScriptAsync("delete Object.getPrototypeOf(navigator).webdriver");

        var baseUrl = Consts.MainUrl;
        if (page.Url == "about:blank")
        {
            try
            {
                await page.GotoAsync(baseUrl, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.Commit,
                    Timeout = 60000
                });
                logger.LogInformation("Page loaded: {url}", baseUrl);
            }
            catch (TimeoutException)
            {
                logger.LogWarning("A timeout occurred while going to the page {url}", baseUrl);
                logger.LogInformation("Cloudflare check failed. Current content: {content}", await page.ContentAsync());
            }

            try
            {
                await page.WaitForFunctionAsync("() => !document.title.includes('Just a moment')");
                logger.LogInformation("Cloudflare check passed.");
            }
            catch (TimeoutException)
            {
                logger.LogWarning("A timeout occurred loading a page {url}", baseUrl);
                logger.LogInformation("Cloudflare check failed. Current content: {content}", await page.ContentAsync());
            }
        }

        return new PlaywrightDisposable(id, playwright, page);
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
