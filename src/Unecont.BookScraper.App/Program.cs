using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Unecont.BookScraper.Core.Helpers;
using Unecont.BookScraper.Core.Models;
using Unecont.BookScraper.Core.Services;
using ILogger = Serilog.ILogger;

internal static class Program
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
    };

    private static async Task Main(string[] args)
    {
        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddHttpClient();
                services.AddSingleton<IBookClient, BookClient>();
                services.AddSerilog(
                    (serviceProvider, configuration) =>
                    {
                        configuration.ReadFrom.Configuration(
                            serviceProvider.GetRequiredService<IConfiguration>()
                        );
                    }
                );
            })
            .Build();

        await host.StartAsync();

        var configuration = host.Services.GetRequiredService<IConfiguration>();
        var categories = configuration.GetSection("Categories").Get<string[]>() ?? [];
        var logger = host.Services.GetRequiredService<ILogger>();

        var categoriesUrls = ScrapingHelper
            .CategoryMap.Where(kvp => categories.Contains(kvp.Key))
            .Select(kvp =>
                $"https://books.toscrape.com/catalogue/category/books/{kvp.Value}/index.html"
            );

        var scrapingTasks = categoriesUrls.Select(u => PageScraper.ScrapeBooks(u, logger));
        var scrapingResults = await Task.WhenAll(scrapingTasks);
        var bookList = scrapingResults.SelectMany(b => b).ToList();

        var minPrice = configuration.GetSection("Filters").GetValue<decimal?>("MinPrice");
        var maxPrice = configuration.GetSection("Filters").GetValue<decimal?>("MaxPrice");
        var rating = configuration.GetSection("Filters").GetValue<int?>("Rating");

        var books = new Books
        {
            BookList = bookList
                .Where(b =>
                    (minPrice is null || b.Price >= minPrice)
                    && (maxPrice is null || b.Price <= maxPrice)
                    && (rating is null || b.Rating == rating)
                )
                .ToList(),
        };

        await SerializationHelper.SerializeToJsonAsync("output.json", books);
        SerializationHelper.SerializeToXml("output.xml", books);

        var client = host.Services.GetRequiredService<IBookClient>();
        using var response = await client.PostBooksAsync(books);
        var status = response.StatusCode;
        var responseContent = await response.Content.ReadAsStringAsync();
        logger.Information("STATUS: {StatusCode}\nRESPONSE: {Response}", status, responseContent);

        await host.StopAsync();
    }
}
