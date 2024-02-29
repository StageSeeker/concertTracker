using StageSeeker.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace StageSeeker.Services;

public class UsersService
{
    private readonly IMongoCollection<User> _usersCollection;
    
    public UsersService(IOptions<MongoDBSettings> stageSeekerDatabaseSettings)
    {
        var mongoClient = new MongoClient(
             stageSeekerDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            stageSeekerDatabaseSettings.Value.DatabaseName);

        _usersCollection = mongoDatabase.GetCollection<User>(
            stageSeekerDatabaseSettings.Value.UserCollectionName);
    }

    // Count users
    private async Task<int> UserCount(){
        try {
        var users = await GetAsync() 
        ?? throw new Exception("Failed to fetch user count");
        return users.Count + 1;
        } catch (Exception) {
            return -1;
        }
    }

    // Get All Users
    public async Task<List<User>> GetAsync()
    {
        try
        {
            return await _usersCollection.Find(_ => true).ToListAsync();
        }
        catch (MongoException ex)
        {
            throw new Exception("Failed to retrieve users: " + ex.Message);
        }

    }

    // GET One Single user
    public async Task<User?> GetAsync(int id)
    {
        var user = _usersCollection.Find(user => user.UserId == id);
        return await user.FirstOrDefaultAsync();
    }

    // Create a user
    public async Task CreateAsync(User new_user)
    {
        try
        {
            int userCount = await UserCount();
            if(userCount <0) {
                throw new Exception("Failed to auto increment userID");
            }
            int uniqueId = userCount++;
            new_user.UserId = uniqueId;
            await _usersCollection.InsertOneAsync(new_user);
        }
        catch (MongoException ex)
        {
            throw new Exception("Failed to create new user" + ex.Message);
        }
    }


    // Delete a user
    public async Task RemoveAsync(int id)
    {
        try
        {
            await _usersCollection.DeleteOneAsync(user => user.UserId == id);
        }
        catch (MongoException ex)
        {
            throw new Exception("Failed to remove user: " + ex.Message);
        }
    }
    public async Task UpdateUserWatchListAsync(int userId, WatchList updateWatchList)
    {
        try
        {
            if (userId < 0 || updateWatchList is null)
            {
                throw new Exception("Invalid User ID or empty WatchList");
            }
            var filter = Builders<User>.Filter.Eq(x => x.UserId, userId);
            var existingUser = await _usersCollection.Find(filter).FirstOrDefaultAsync();

            if (existingUser is null) {
                throw new Exception("User was not found");
            }

            existingUser.WatchList.Add(updateWatchList);
            var replaceResult = await _usersCollection.ReplaceOneAsync(filter, existingUser);

            if(!replaceResult.IsModifiedCountAvailable) {
                throw new Exception($"Failed to update Watchlist for user: {userId}");
            }
            Console.WriteLine($"Succesfully update user with ID: {userId}");
        }
        catch(Exception ex)
        {
            Console.WriteLine("Failed to update user watchlist " + ex.Message);
        }
    }

    public async Task<User?> GetUserByName(string username)
    {
        var user = _usersCollection.Find(user => user.Username == username);
        return await user.FirstOrDefaultAsync();
    } 
}


