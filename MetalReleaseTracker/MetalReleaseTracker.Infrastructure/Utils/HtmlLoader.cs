﻿using HtmlAgilityPack;
using MetalReleaseTracker.Infrastructure.Providers;

namespace MetalReleaseTracker.Infrastructure.Utils
{
    public class HtmlLoader : IHtmlLoader
    {
        private readonly HttpClient _httpClient;
        private readonly UserAgentProvider _userAgentProvider;

        public HtmlLoader(HttpClient httpClient, UserAgentProvider userAgentProvider)
        {
            _httpClient = httpClient;
            _userAgentProvider = userAgentProvider;
        }

        public async Task<HtmlDocument> LoadHtmlDocumentAsync(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.UserAgent.ParseAdd(_userAgentProvider.GetRandomUserAgent());

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var pageContents = await response.Content.ReadAsStringAsync();
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(pageContents);

            return htmlDocument;
        }
    }
}
