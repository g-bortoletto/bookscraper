using System.Globalization;
using AngleSharp;
using AngleSharp.Dom;
using Serilog;
using Unecont.BookScraper.Core.Helpers;
using Unecont.BookScraper.Core.Models;

namespace Unecont.BookScraper.Core.Services;

public static class PageScraper
{
    public static async Task<IEnumerable<Book>> ScrapeBooks(
        string firstPageUrl,
        ILogger? logger = null
    )
    {
        logger.Information("Starting book scraping from: {FirstPageUrl}", firstPageUrl);
        var allPagesUrls = await GetPagesUrls(firstPageUrl, logger);
        logger.Information("Found {Count} pages to scrape.", allPagesUrls.Count);
        var scrapingTasks = allPagesUrls.Select(url => ScrapePage(url, logger));
        var scrapingResults = await Task.WhenAll(scrapingTasks);
        logger.Information("Finished scraping all pages.");
        return scrapingResults.SelectMany(b => b);
    }

    private static readonly IConfiguration PageScraperConfiguration =
        Configuration.Default.WithDefaultLoader();

    private static async Task<HashSet<string>> GetPagesUrls(string pageUrl, ILogger? logger = null)
    {
        logger.Information("Getting all page URLs starting from: {PageUrl}", pageUrl);
        HashSet<string> pagesUrl = [pageUrl];
        using var context = BrowsingContext.New(PageScraperConfiguration);
        using var document = await context.OpenAsync(pageUrl);

        var nextPageLink = document.QuerySelector("li.next a")?.GetAttribute("href");
        while (!string.IsNullOrWhiteSpace(nextPageLink))
        {
            var baseUri = new Uri(pageUrl, UriKind.Absolute);
            var absoluteUri = new Uri(baseUri, nextPageLink);
            string nextPageUrl = absoluteUri.ToString();
            pagesUrl.Add(nextPageUrl);
            logger.Information("Found next page: {NextPageUrl}", nextPageUrl);

            using var nextPageDocument = await context.OpenAsync(nextPageUrl);
            nextPageLink = nextPageDocument.QuerySelector("li.next a")?.GetAttribute("href");
        }

        logger.Information("Total pages found: {PagesUrlCount}", pagesUrl.Count);
        return pagesUrl;
    }

    private static async Task<IEnumerable<Book>> ScrapePage(string pageUrl, ILogger? logger = null)
    {
        logger.Information("Scraping page: {PageUrl}", pageUrl);
        using var context = BrowsingContext.New(PageScraperConfiguration);
        using var document = await context.OpenAsync(pageUrl);

        string category = GetCategory(document);
        var productPods = document.QuerySelectorAll("ol.row article.product_pod");

        var books = productPods
            .Select(productPod =>
            {
                string title = GetTitle(productPod);
                decimal price = GetPrice(productPod);
                int rating = GetRating(productPod);
                string url = GetUrl(productPod, pageUrl);

                logger.Debug(
                    "Parsed book: {Title}, Price: {Price}, Rating: {Rating}, Category: {Category}, Url: {Url}",
                    title,
                    price,
                    rating,
                    category,
                    url
                );

                return new Book
                {
                    Title = title,
                    Price = price,
                    Rating = rating,
                    Category = category,
                    Url = url,
                };
            })
            .ToList();
        logger.Information("Scraped {Count} books from page: {PageUrl}", books.Count, pageUrl);
        return books;
    }

    private static string GetCategory(IDocument document)
    {
        return document.QuerySelectorAll("div.page-header.action h1").FirstOrDefault()?.TextContent
            ?? string.Empty;
    }

    private static string GetTitle(IElement element)
    {
        return element.QuerySelector("h3 a")?.GetAttribute("title") ?? string.Empty;
    }

    private static decimal GetPrice(IElement element)
    {
        string fullPrice =
            element.QuerySelector("div.product_price p.price_color")?.TextContent ?? string.Empty;
        var match = ScrapingHelper.PriceNumber().Match(fullPrice);
        return
            match.Success
            && decimal.TryParse(match.Value, CultureInfo.InvariantCulture, out decimal price)
            ? price
            : decimal.Zero;
    }

    private static int GetRating(IElement element)
    {
        var classTokens =
            element.QuerySelector("p.star-rating")?.ClassList ?? Enumerable.Empty<string>();
        foreach (var classToken in classTokens)
        {
            if (ScrapingHelper.TryParseRating(classToken, out var rating))
            {
                return rating!.Value;
            }
        }
        return 0;
    }

    private static string GetUrl(IElement element, string baseUrl)
    {
        var href = element.QuerySelector("h3 a")?.GetAttribute("href") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(href))
        {
            return string.Empty;
        }

        var baseUri = new Uri(baseUrl, UriKind.Absolute);
        var absoluteUri = new Uri(baseUri, href);
        return absoluteUri.ToString();
    }
}
