using StageSeeker.Models;

namespace StageSeeker.Services;

public static class UserService {
    static List<User> Users {get;}
    static UserService() {
        Users = new List<User>{
            new User {Id = 1, Username = "Lavon", Password = "Concert"}
        };
    }
    
    // Routes
    public static List<User> GetAll() => Users;
}