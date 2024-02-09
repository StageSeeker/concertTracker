using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StageSeeker.Models;

public class WatchList {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ObjectId {get; set;}
    [BsonElement("user_id")]
    public int UserId {get; set;}
    [BsonElement("watch_id")]
    public int WatchId {get; set;}
    [BsonElement("artist_name")]
    public required string ArtistName {get; set;}
    [BsonElement("concert_name")]
    public required string ConcertName {get; set;}
    [BsonElement("venue")]
    public required string Venue {get; set;}
    [BsonElement("time")]
    public DateTime Time {get; set;}
    [BsonElement("Ticket_price")]
    public int Price {get; set;}
    [BsonElement("Attending")]
    public bool IsAttending {get; set;}
}