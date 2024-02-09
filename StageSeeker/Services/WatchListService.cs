using StageSeeker.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

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
}
