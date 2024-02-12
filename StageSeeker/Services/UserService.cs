using StageSeeker.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace StageSeeker.Services;

public class UsersService 
{
    private readonly IMongoCollection<User> _usersCollection;

    public UsersService (IOptions<MongoDBSettings> stageSeekerDatabaseSettings) 
    {
        var mongoClient = new MongoClient(
             stageSeekerDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            stageSeekerDatabaseSettings.Value.DatabaseName);

        _usersCollection = mongoDatabase.GetCollection<User>(
            stageSeekerDatabaseSettings.Value.UserCollectionName);
    }

    // Get All Users
    public async Task<List<User>> GetAsync() 
    {
        try {
            return await _usersCollection.Find(_ => true).ToListAsync();
        } catch (MongoException ex) {
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
        try {
            await _usersCollection.InsertOneAsync(new_user);
        } 
        catch (MongoException ex) {
            throw new Exception("Failed to create new user" + ex.Message);
        }
    }


    // Delete a user
    public async Task RemoveAsync(int id) {
        try {
            await _usersCollection.DeleteOneAsync(user => user.UserId == id);
        } catch (MongoException ex) {
            throw new Exception("Failed to remove user: " + ex.Message);
        }
    }
}


