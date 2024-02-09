using StageSeeker.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

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
  public async Task UpdateAsync(int id, bool isAttending)
{
    // Use Builders to filter the WatchList by WatchId
    var filter = Builders<WatchList>.Filter.Eq(x => x.WatchId, id);

    // Create the update operation for IsAttending
    var update = Builders<WatchList>.Update.Set(w => w.IsAttending, isAttending);

    // Perform the update
    await _watchListCollection.UpdateOneAsync(filter, update);
}

    // Remove WatchList
    public async Task RemoveAsync(int id) {
        try {
            await _watchListCollection.DeleteOneAsync(x=>x.WatchId == id);
        } catch (MongoException ex)
    {
        // Handle errors during watch list retrieval
        throw new Exception("Failed to to remove watch lists: " + ex.Message);
    }
    }
}


