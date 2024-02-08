namespace StageSeeker.Models;

public class User {
    public long Id {get; set;}
    public required string Username {get; set;}
    public required string Password {get; set;}
}