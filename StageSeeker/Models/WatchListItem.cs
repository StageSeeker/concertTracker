using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StageSeeker.Models;

public class WatchListItem {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ObjectId {get; set;}
    [BsonElement("user_id")]
    public int UserId {get; set;}
    [BsonElement("concert_id")]
    public string? ConcertId {get; set;}
    [BsonElement("Favorite")]
    public string? WatchlistId {get; set;}
    [BsonElement("artist_name")]
    public string ArtistName {get; set;} = null!;
    [BsonElement("concert_name")]
    public string ConcertName {get; set;} = null!;
    [BsonElement("venue")]
    public string Venue {get; set;} = null!;
    [BsonElement("time")]
    public DateTime Time {get; set;}
    [BsonElement("Ticket_price")]
    public int Price {get; set;}
    [BsonElement("Attending")]
    public bool IsAttending {get; set;}
}