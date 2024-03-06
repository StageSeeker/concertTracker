using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StageSeeker.Models;

public class WatchList {
    
    [BsonElement("Favorite")]
    public string WatchlistId { get; set; } = Guid.NewGuid().ToString(); // Unique identifier for the watchlist
    
    [BsonElement("items")]
    public List<WatchListItem> Items { get; set; } = new List<WatchListItem>();
}