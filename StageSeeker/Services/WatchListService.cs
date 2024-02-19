using StageSeeker.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Microsoft.AspNetCore.Mvc;
using StageSeeker.Utilities;

namespace StageSeeker.Services;
public class WatchListService
{
    private readonly IMongoCollection<WatchList> _watchListCollection;

    public WatchListService(IOptions<MongoDBSettings> mongoDBSettings)
    {
        try
        {
            // Access settings directly (no string checks)
            var mongoClient = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var mongoDataBase = mongoClient.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _watchListCollection = mongoDataBase.GetCollection<WatchList>(mongoDBSettings.Value.WatchListCollectionName);
        }
        catch (MongoException ex)
        {
            // Handle MongoDB connection and initialization errors
            throw new Exception("Failed to connect to MongoDB: " + ex.Message);
        }
    }
    // Get All WatchLists
    public async Task<List<WatchList>> GetWatchAsync()
    {
        try
        {
            return await _watchListCollection.Find(_ => true).ToListAsync();
        }
        catch (MongoException ex)
        {
            // Handle errors during watch list retrieval
            throw new Exception("Failed to retrieve watch lists: " + ex.Message);
        }
    }
    //Get One WatchList
    public async Task<WatchList?> GetWatchAsync(int id)
    {
        var cursor = _watchListCollection.Find(x => x.WatchId == id);
        return await cursor.FirstOrDefaultAsync();
    }
    // Create WatchList
    public async Task CreateAsync(WatchList new_watchList)
    {
        try
        {
            await _watchListCollection.InsertOneAsync(new_watchList);
        }
        catch (MongoException ex)
        {
            // Handle errors during watch list retrieval
            throw new Exception("Failed to to create watch lists: " + ex.Message);
        }
    }
    // Update WatchList
    public async Task UpdateAsync(int id, [FromBody] WatchList updateList)
    {
        var filter = Builders<WatchList>.Filter.Eq(x => x.WatchId, id);
        var update = Builders<WatchList>.Update.Combine();

        // Conditionally sets fields based on null inputs
        // String fields needs to be emtpty "" to remain the same. 
        if (!string.IsNullOrEmpty(updateList.ArtistName))
        {
            update = update.Set(x => x.ArtistName, updateList.ArtistName);
        }
        if (!string.IsNullOrEmpty(updateList.ConcertName))
        {
            update = update.Set(x => x.ConcertName, updateList.ConcertName);
        }
        if (!string.IsNullOrEmpty(updateList.Venue))
        {
            update = update.Set(x => x.Venue, updateList.ConcertName);
        }
        // Price field can be 0 to keep the same value or null(empty)
        if (updateList.Price != 0)
        {
            update = update.Set(x => x.Price, updateList.Price);
        }
        if (updateList.IsAttending)
        {
            update = update.Set(x => x.IsAttending, updateList.IsAttending);
        }
        await _watchListCollection.UpdateOneAsync(filter, update);
    }

    // Remove WatchList
    public async Task RemoveAsync(int id)
    {
        try
        {
            await _watchListCollection.DeleteOneAsync(x => x.WatchId == id);
        }
        catch (MongoException ex)
        {
            // Handle errors during watch list retrieval
            throw new Exception("Failed to to remove watch lists: " + ex.Message);
        }
    }
}


