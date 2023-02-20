using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace TMLeague.Http
{
    public sealed class DefaultBrowserOptionsMessageHandler : DelegatingHandler
    {
        public DefaultBrowserOptionsMessageHandler()
        { }

        public DefaultBrowserOptionsMessageHandler(HttpMessageHandler innerHandler)
        {
            InnerHandler = innerHandler;
        }

        public BrowserRequestCache? DefaultBrowserRequestCache { get; init; }
        public BrowserRequestCredentials? DefaultBrowserRequestCredentials { get; init; }
        public BrowserRequestMode? DefaultBrowserRequestMode { get; init; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Get the existing options to not override them if set explicitly

            if (DefaultBrowserRequestCache.HasValue)
                request.SetBrowserRequestCache(DefaultBrowserRequestCache.Value);

            if (DefaultBrowserRequestCredentials.HasValue)
                request.SetBrowserRequestCredentials(DefaultBrowserRequestCredentials.Value);

            if (DefaultBrowserRequestMode.HasValue)
                request.SetBrowserRequestMode(DefaultBrowserRequestMode.Value);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
