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

    public async Task<List<User>> GetAsync() =>
        await _usersCollection.Find(_ => true).ToListAsync();
}


