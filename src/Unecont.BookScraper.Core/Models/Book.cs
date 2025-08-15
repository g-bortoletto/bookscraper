using System.Linq.Expressions;
using System.Xml.Serialization;

namespace Unecont.BookScraper.Core.Models;

public class Book
{
    [XmlAttribute("title")]
    public string Title { get; set; } = null!;

    [XmlAttribute("price")]
    public decimal Price { get; set; }

    [XmlAttribute("rating")]
    public int Rating { get; set; }

    [XmlAttribute("category")]
    public string Category { get; set; } = null!;

    [XmlAttribute("url")]
    public string Url { get; set; } = null!;
}

[XmlRoot("Books")]
public class Books
{
    [XmlElement("Book")]
    public List<Book> BookList { get; set; } = [];
}

public class BookFilters
{
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? Rating { get; set; }
}
