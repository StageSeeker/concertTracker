using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace StageSeeker.Models;

public class ConcertResponse {
    [JsonProperty("events")]
    public required List<Concert> Concerts {get; set;}
}

public class Concert {
    [JsonProperty("id")]
    public int ConcertId {get; set;}
    [JsonProperty("title")]
    public string Title {get; set;} = null!;
    [JsonProperty("datetime_local")]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime Date {get; set;}
    [JsonProperty("venue")]
    public required Venue Location {get; set;}
    [JsonProperty("stats")]
    public required Stats Prices {get; set;}
    [JsonProperty("performers")]
    public required List<Performer> Performers {get; set;}
    [JsonProperty("artistNames")]
    public string[] ArtistNames {get; set;} = null!;
}

public class Venue {
    [JsonProperty("name")]
    public string Name {get; set;} = null!; 
}

public class Stats {
    [JsonProperty("lowest_sg_base_price")]
    public int LowestPrice {get; set;}
}

public class Performer {
    [JsonProperty("short_name")]
    public required string Artist {get; set;}
}





