using StageSeeker.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Microsoft.AspNetCore.Mvc;

namespace StageSeeker.Services;
public class WatchListService
{
    private readonly IMongoCollection<WatchList> _watchListCollection;
    private readonly ConcertService _concertService;

    public WatchListService(IOptions<MongoDBSettings> mongoDBSettings, ConcertService concertService)
    {
        try
        {
            // Access settings directly (no string checks)
            var mongoClient = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var mongoDataBase = mongoClient.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _watchListCollection = mongoDataBase.GetCollection<WatchList>(mongoDBSettings.Value.WatchListCollectionName);

            _concertService = concertService;
        }
        catch (MongoException ex)
        {
            // Handle MongoDB connection and initialization errors
            throw new Exception("Failed to connect to MongoDB: " + ex.Message);
        }
    }
    // Get All User WatchLists
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
    //Get One WatchList by the user id
    public async Task<WatchList?> GetUserWatchAsync(int userId)
    {
        var cursor = _watchListCollection.Find(x => x.UserId == userId);
        return await cursor.FirstOrDefaultAsync();
    }

    // Get One Concert on User WatchList
    public async Task<WatchList> GetOneUserConcertAsync(int userId, int concertId) {
        var concertItem = Builders<WatchList>.Filter.And(
        Builders<WatchList>.Filter.Eq(x => x.UserId, userId),
        Builders<WatchList>.Filter.Eq(x => x.WatchId, concertId));
        return await _watchListCollection.Find(concertItem).FirstOrDefaultAsync();
    }
    // Create WatchList 
    public async Task CreateAsync(int userId, string artist, int concertId)
    {
        try
        {
            List<Concert> concerts = await _concertService.GetConcertsByArtist(artist);
            Concert desiredConcert = concerts.FirstOrDefault(x => x.ConcertId == concertId) 
            ?? throw new Exception($"Concert with ID {concertId} was not found for {artist}");
            var watchList = new WatchList
        {
            UserId = userId,
            WatchId = desiredConcert.ConcertId,
            ArtistName = string.Join(", ", desiredConcert.Performers.Select(p => p.Artist)),
            ConcertName = desiredConcert.Title,
            Venue = desiredConcert.Location.Name,
            Time = desiredConcert.Date,
            Price = desiredConcert.Prices.LowestPrice,
            IsAttending = false // Initially set to false
        };
        if(watchList is null) {
            throw new Exception($"Failed to create WatchList object for concert: {concertId}");
        }
            await _watchListCollection.InsertOneAsync(watchList);
        }
        catch (MongoException ex)
        {
            // Handle errors during watch list retrieval
            throw new Exception("Failed to to create watch lists: " + ex.Message);
        }
    }
    // Update WatchList
    public async Task UpdateAsync(int userId, int concertId, [FromBody] WatchList updateList)
    {
        var filter = Builders<WatchList>.Filter.And(
        Builders<WatchList>.Filter.Eq(x => x.UserId, userId),
        Builders<WatchList>.Filter.Eq(x => x.WatchId, concertId));
        var update = Builders<WatchList>.Update.Combine();

        // Conditionally sets fields based on null inputs
        // String fields needs to be emtpty "" to remain the same. 
        // if (!string.IsNullOrEmpty(updateList.ArtistName))
        // {
        //     update = update.Set(x => x.ArtistName, updateList.ArtistName);
        // }
        // if (!string.IsNullOrEmpty(updateList.ConcertName))
        // {
        //     update = update.Set(x => x.ConcertName, updateList.ConcertName);
        // }
        // if (!string.IsNullOrEmpty(updateList.Venue))
        // {
        //     update = update.Set(x => x.Venue, updateList.ConcertName);
        // }
        // // Price field can be 0 to keep the same value or null(empty)
        // if (updateList.Price != 0)
        // {
        //     update = update.Set(x => x.Price, updateList.Price);
        // }
        if (updateList.IsAttending)
        {
            update = update.Set(x => x.IsAttending, updateList.IsAttending);
        }
        await _watchListCollection.UpdateOneAsync(filter, update);
    }

    // Remove WatchList
    public async Task RemoveAsync(int concertId)
    {
        try
        {
            await _watchListCollection.DeleteOneAsync(x => x.WatchId == concertId);
        }
        catch (MongoException ex)
        {
            // Handle errors during watch list retrieval
            throw new Exception("Failed to to remove watch lists: " + ex.Message);
        }
    }
}
