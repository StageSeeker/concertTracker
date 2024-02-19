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
    [JsonProperty("name")]
    public required string Artist {get; set;}
    [JsonProperty("url")]
    public string TicketsURL {get;set;} = null!;
    [JsonProperty("num_upcoming_events")]
    public int NumberOfEvents {get; set;}
}





