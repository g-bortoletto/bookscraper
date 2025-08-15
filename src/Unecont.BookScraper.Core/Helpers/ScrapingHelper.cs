using System.Text.RegularExpressions;

namespace Unecont.BookScraper.Core.Helpers;

public static partial class ScrapingHelper
{
    public static bool TryParseRating(string value, out int? result)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        switch (value.ToLowerInvariant())
        {
            case "one":
                result = 1;
                return true;
            case "two":
                result = 2;
                return true;
            case "three":
                result = 3;
                return true;
            case "four":
                result = 4;
                return true;
            case "five":
                result = 5;
                return true;
            default:
                result = null;
                return false;
        }
    }

    [GeneratedRegex(@"[0-9]+(?:\.[0-9]+)?")]
    public static partial Regex PriceNumber();

    public static Dictionary<string, string> CategoryMap { get; } =
        new()
        {
            ["travel"] = "travel_2",
            ["mystery"] = "mystery_3",
            ["historical fiction"] = "historical-fiction_4",
            ["sequential art"] = "sequential-art_5",
            ["classics"] = "classics_6",
            ["philosophy"] = "philosophy_7",
            ["romance"] = "romance_8",
            ["womens fiction"] = "womens-fiction_9",
            ["fiction"] = "fiction_10",
            ["childrens"] = "childrens_11",
            ["religion"] = "religion_12",
            ["nonfiction"] = "nonfiction_13",
            ["music"] = "music_14",
            ["default"] = "default_15",
            ["science fiction"] = "science-fiction_16",
            ["sports and games"] = "sports-and-games_17",
            ["add a comment"] = "add-a-comment_18",
            ["fantasy"] = "fantasy_19",
            ["new adult"] = "new-adult_20",
            ["young adult"] = "young-adult_21",
            ["science"] = "science_22",
            ["poetry"] = "poetry_23",
            ["paranormal"] = "paranormal_24",
            ["art"] = "art_25",
            ["psychology"] = "psychology_26",
            ["autobiography"] = "autobiography_27",
            ["parenting"] = "parenting_28",
            ["adult fiction"] = "adult-fiction_29",
            ["humor"] = "humor_30",
            ["horror"] = "horror_31",
            ["history"] = "history_32",
            ["food and drink"] = "food-and-drink_33",
            ["christian fiction"] = "christian-fiction_34",
            ["business"] = "business_35",
            ["biography"] = "biography_36",
            ["thriller"] = "thriller_37",
            ["contemporary"] = "contemporary_38",
            ["spirituality"] = "spirituality_39",
            ["academic"] = "academic_40",
            ["self help"] = "self-help_41",
            ["historical"] = "historical_42",
            ["christian"] = "christian_43",
            ["suspense"] = "suspense_44",
            ["short stories"] = "short-stories_45",
            ["novels"] = "novels_46",
            ["health"] = "health_47",
            ["politics"] = "politics_48",
            ["cultural"] = "cultural_49",
            ["erotica"] = "erotica_50",
            ["crime"] = "crime_51",
        };
}
