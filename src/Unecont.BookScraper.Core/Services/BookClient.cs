using System.Net.Http.Json;
using Unecont.BookScraper.Core.Models;

namespace Unecont.BookScraper.Core.Services;

public interface IBookClient
{
    public Task<HttpResponseMessage> PostBooksAsync(Books books);
}

public class BookClient : IBookClient
{
    private readonly HttpClient _http;

    public BookClient(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient();
        _http.BaseAddress = new Uri("https://httpbin.org");
    }

    public async Task<HttpResponseMessage> PostBooksAsync(Books books)
    {
        return await _http.PostAsJsonAsync("/post", books);
    }
}
