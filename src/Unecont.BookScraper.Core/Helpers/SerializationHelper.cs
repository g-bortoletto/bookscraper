using System.Text.Json;
using System.Xml.Serialization;

namespace Unecont.BookScraper.Core.Helpers;

public static class SerializationHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static async Task SerializeToJsonAsync<T>(string filePath, T data)
    {
        string json = JsonSerializer.Serialize(data, JsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    public static void SerializeToXml<T>(string filePath, T data)
    {
        XmlSerializer xmlSerializer = new(typeof(T));
        using StreamWriter writer = new(File.Create(filePath));
        xmlSerializer.Serialize(writer, data);
    }
}
