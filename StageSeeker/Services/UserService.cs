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
        try {
            var user = _usersCollection.Find(user => user.UserId == id);
        var projection = Builders<User>.Projection.Include(x => x.WatchList);
        if (user is null) {
            throw new Exception($"Failed to find user with ID {id}");
        }
       
        return await user.SingleAsync();
        } catch(MongoException ex) {
            throw new Exception("Error: " + ex.Message);
        }
        
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
            if(new_user.WatchList is null) {
                new_user.WatchList = [];
            }
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

    // Helper Method to Delete Concert from User WatchList
    public async Task RemoveConcertAsync(int userId, int concertId) {
        try {
            var user = await _usersCollection.FindOneAndUpdateAsync(
                Builders<User>.Filter.Eq(x=>x.UserId, userId),
                Builders<User>.Update.PullFilter(x=> x.WatchList,
                Builders<WatchList>.Filter.Eq(y=> y.WatchId, concertId))
            );
            if(user is null) {
                throw new Exception($"Failed to find concert ID {concertId}, for user {userId} - {user?.Username}");
            }
        } catch (MongoException ex) {
            throw new Exception("Error: " + ex.Message);
        }
    }

    //Helper Method tto obtain user Name when logged via Auth0    
    public async Task<User?> GetUserByName(string username)
    {
        try {
            var user = _usersCollection.Find(user => user.Username == username);
            if (user is null) {
                throw new Exception($"Failed to find user: {user}");
            }
        return await user.FirstOrDefaultAsync();
        } catch(Exception ex) {
            throw new Exception("Error: " + ex.Message);
        }  
    }

    // Helper Method to update WatchList
    public async Task UpdateUserWatchListAsync(int userId, int concertId, bool isAttending)
{
    try
    {
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(x => x.UserId, userId),
            Builders<User>.Filter.ElemMatch(x => x.WatchList, wl => wl.WatchId == concertId && !wl.IsAttending)
        );

        var update = Builders<User>.Update
            .Set("WatchList.$.IsAttending", isAttending);

        var updateResult = await _usersCollection.UpdateOneAsync(filter, update);

        if (updateResult.ModifiedCount == 0)
        {
            throw new Exception($"Failed to update user watchlist: Entry with WatchId {concertId} not found for user ID {userId}");
        }
    }
    catch (MongoException ex)
    {
        throw new Exception("Failed to update user watchlist: " + ex.Message);
    }
}

}


