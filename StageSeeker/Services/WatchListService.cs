using StageSeeker.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Microsoft.AspNetCore.Mvc;

namespace StageSeeker.Services;
public class WatchListService
{
    private readonly IMongoCollection<WatchList> _watchListCollection;
    private readonly IMongoCollection<User> _usersCollection;
    private readonly ConcertService _concertService;
    private readonly UsersService _userService;

    public WatchListService(IOptions<MongoDBSettings> mongoDBSettings, ConcertService concertService, UsersService userService)
    {
        try
        {
            // Access settings directly (no string checks)
            var mongoClient = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var mongoDataBase = mongoClient.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _watchListCollection = mongoDataBase.GetCollection<WatchList>(mongoDBSettings.Value.WatchListCollectionName);
            _usersCollection = mongoDataBase.GetCollection<User>(mongoDBSettings.Value.UserCollectionName);
            _concertService = concertService;
            _userService = userService;
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
    public async Task<List<WatchList>> GetUserWatchAsync(int userId)
    {
        try {
        var cursor = _watchListCollection.Find(x => x.UserId == userId);
        if(cursor is null) {
            throw new Exception($"Failed to get watch list for user ID:{userId}");
        }
        return await cursor.ToListAsync();
        } catch(MongoException ex) {
            throw new Exception("Error: " + ex.Message);
        }
        
    }

    // Get One Concert on User WatchList
    public async Task<WatchList?> GetOneUserConcertAsync(int userId, int concertId) {
        try {
            var user = await _userService.GetAsync(userId);
            if(user is null) {
                throw new Exception($"Failed to find user {userId}");
            }
            var watchlist = user.WatchList?.FirstOrDefault(x=> x.WatchId == concertId);
            if(watchlist is null) {
                throw new Exception($"Failed to find user concert {concertId}");
            }
            return watchlist;
        } catch (Exception ex) {
            throw new Exception("Error: " + ex.Message);
        }
    }

    // Create WatchList 
    public async Task<WatchList> CreateAsync(int userId, string artist, int concertId)
    {
        try
        {
            var existingConcert = await _watchListCollection.Find(
                Builders<WatchList>.Filter.Eq(x => x.UserId, userId) &
                Builders<WatchList>.Filter.Eq(x=>x.WatchId, concertId)).FirstOrDefaultAsync();
            if(existingConcert != null) {
                throw new Exception($"You already have this concert ({artist} - {concertId}) in your watchlist.");
            }
            
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
            await AddToUserWatchListAsync(userId, watchList);
            return watchList;
        }
        catch (MongoException ex)
        {
            // Handle errors during watch list retrieval
            throw new Exception("Failed to to create watch lists: " + ex.Message);
        }
    }
    public async Task AddToUserWatchListAsync(int userId, WatchList newConcert)
        {
            try {
            var user = await _usersCollection.Find(Builders<User>.Filter.Eq(x => x.UserId, userId))
            .FirstOrDefaultAsync();            
            if(user is null) {
                throw new Exception($"User with ID {userId} not found.");
            }
            List<WatchList> existingWatchList = user.WatchList;
            if(existingWatchList is null) {
                existingWatchList = [];
            }
            existingWatchList.Add(newConcert);
            user.WatchList = existingWatchList;
            await _usersCollection.ReplaceOneAsync(
            Builders<User>.Filter.Eq(x => x.UserId, userId),
            user);
        } catch(MongoException ex) {
            throw new Exception("Failed to update WatchList: " + ex.Message);
        }
        }

    // Update WatchList
    public async Task UpdateAsync(int userId, int concertId, bool attendance)
    {
        var filter = Builders<WatchList>.Filter.And(
        Builders<WatchList>.Filter.Eq(x => x.UserId, userId),
        Builders<WatchList>.Filter.Eq(x => x.WatchId, concertId));
        var update = Builders<WatchList>.Update.Set(x=>x.IsAttending, attendance);
        
        try {
            await _watchListCollection.UpdateOneAsync(filter, update);
        await _userService.UpdateUserWatchListAsync(userId, concertId, attendance);
        } catch(MongoException ex) {
            throw new Exception("Failed to update watch list: " + ex.Message);
        }
        
        
    }

    // Remove WatchList
    public async Task RemoveAsync(int userId, int concertId)
    {
        try
        {
            await _watchListCollection.DeleteOneAsync(x => x.WatchId == concertId);
            await _userService.RemoveConcertAsync(userId, concertId);
        }
        catch (MongoException ex)
        {
            // Handle errors during watch list retrieval
            throw new Exception("Failed to to remove watch lists: " + ex.Message);
        }
    }
}
