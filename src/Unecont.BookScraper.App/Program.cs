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
        var logger = host.Services.GetRequiredService<ILogger>();

        try
        {
            var categories = GetCategories(configuration);
            if (categories.Length == 0)
            {
                logger.Warning("No categories configured. Nothing to scrape.");
            }

            var categoriesUrls = BuildCategoriesUrls(categories);

            if (categoriesUrls.Length == 0)
            {
                logger.Warning("No valid category URLs resolved from configuration.");
            }

            BookFilters bookFilters = GetBookFilters(configuration);

            var scrapingTasks = categoriesUrls.Select(u =>
                PageScraper.ScrapeBooks(u, bookFilters, logger)
            );
            var scrapingResults = await Task.WhenAll(scrapingTasks);
            var bookList = scrapingResults.SelectMany(b => b).ToList();

            var books = new Books { BookList = bookList };

            EnsureOutDirectoryExists();
            await SerializationHelper.SerializeToJsonAsync("out/books.json", books);
            SerializationHelper.SerializeToXml("out/books.xml", books);

            logger.Information(
                "Prepared {Count} books. Categories: {Categories}. Filters => MinPrice: {MinPrice}, MaxPrice: {MaxPrice}, Rating: {Rating}",
                books.BookList.Count,
                string.Join(", ", categories),
                bookFilters.MinPrice,
                bookFilters.MaxPrice,
                bookFilters.Rating
            );

            var client = host.Services.GetRequiredService<IBookClient>();
            using var response = await client.PostBooksAsync(books);
            var status = response.StatusCode;
            var sample = string.Join(
                " | ",
                books.BookList.Take(5).Select(b => $"{b.Title} ({b.Price})")
            );
            if (response.IsSuccessStatusCode)
            {
                logger.Information(
                    "POST to https://httpbin.org/post returned {StatusCode}. Sent {Count} books. Sample: {Sample}",
                    status,
                    books.BookList.Count,
                    sample
                );
            }
            else
            {
                logger.Error(
                    "POST to https://httpbin.org/post failed with {StatusCode}. Attempted to send {Count} books.",
                    status,
                    books.BookList.Count
                );
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "An unhandled error occurred during execution.");
        }
        finally
        {
            await host.StopAsync();
        }
    }

    private static string[] BuildCategoriesUrls(string[] categories)
    {
        return ScrapingHelper
            .CategoryMap.Where(kvp => categories.Contains(kvp.Key))
            .Select(kvp =>
                $"https://books.toscrape.com/catalogue/category/books/{kvp.Value}/index.html"
            )
            .ToArray();
    }

    private static string[] GetCategories(IConfiguration configuration)
    {
        return configuration.GetSection("Categories").Get<string[]>() ?? [];
    }

    private static BookFilters GetBookFilters(IConfiguration configuration)
    {
        return new()
        {
            MinPrice = configuration.GetSection("Filters").GetValue<decimal?>("MinPrice"),
            MaxPrice = configuration.GetSection("Filters").GetValue<decimal?>("MaxPrice"),
            Rating = configuration.GetSection("Filters").GetValue<int?>("Rating"),
        };
    }

    private static void EnsureOutDirectoryExists()
    {
        if (!Directory.Exists("out"))
        {
            Directory.CreateDirectory("out");
        }
    }
}
