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

    [BsonElement("email")]
    public string Email {get; set;} = null!;
    
    public string ProfilePic {get; set;} = null!;

    [BsonElement("watch_lists")]
    public List<WatchList> WatchLists {get; set;} = new List<WatchList>();
}