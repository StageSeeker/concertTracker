using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace StageSeeker.Models;

public class User {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("user_id")]
    public int UserId { get; set; }

    [BsonElement("username")]
    public required string Username { get; set; }
    
    [BsonElement("password")]
    public required string Password { get; set; }

    [BsonElement("WatchList")]
    public WatchList WatchList {get; set;} = new WatchList();
}