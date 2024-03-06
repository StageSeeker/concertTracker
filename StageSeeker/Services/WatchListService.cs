using StageSeeker.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Microsoft.AspNetCore.Mvc;

namespace StageSeeker.Services;
public class WatchListService
{
    private readonly IMongoCollection<WatchList> _watchListCollection;
    private readonly IMongoCollection<WatchListItem> _watchListItemCollection;
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
            _watchListItemCollection = mongoDataBase.GetCollection<WatchListItem>(mongoDBSettings.Value.WatchListCollectionName);
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
    // Get all watch lists for a user
    public async Task<List<WatchList>> GetAllUserWatchListsAsync(int userId)
    {
        try
        {
            var user = await _userService.GetAsync(userId) ?? throw new Exception("Failed to find user");
            var watchlists = new List<WatchList>();
            // return user?.WatchLists ?? new List<WatchList>();
            foreach (var watchListId in user.WatchLists.Select(wl => wl.WatchlistId).Distinct())
        {
            var items = user.WatchLists.Where(wl => wl.WatchlistId == watchListId).Select(wl => wl.Items).FirstOrDefault();
            watchlists.Add(new WatchList
            {
                WatchlistId = watchListId,
                Items = items ?? []
            });
        }

        return watchlists;
        }
        catch (MongoException ex)
        {
            throw new Exception("Failed to retrieve all watch lists for the user: " + ex.Message);
        }
    }
    //Get One WatchList by the user id
    public async Task<List<WatchListItem>> GetUserWatchAsync(int userId)
    {
        try
        {
            var cursor = _watchListItemCollection.Find(x => x.UserId == userId);
            if (cursor is null)
            {
                throw new Exception($"Failed to get watch list for user ID:{userId}");
            }
            return await cursor.ToListAsync();
        }
        catch (MongoException ex)
        {
            throw new Exception("Error: " + ex.Message);
        }

    }

    // Get One Concert on User WatchList
    public async Task<WatchList?> GetOneUserConcertAsync(int userId, string concertId)
    {
        try
        {
            var user = await _userService.GetAsync(userId);
            if (user is null)
            {
                throw new Exception($"Failed to find user {userId}");
            }
            var watchlist = user.WatchLists.FirstOrDefault(x => x.WatchlistId == concertId);
            if (watchlist is null)
            {
                throw new Exception($"Failed to find user concert {concertId}");
            }
            return watchlist;
        }
        catch (Exception ex)
        {
            throw new Exception("Error: " + ex.Message);
        }
    }

    // Create WatchList 
    public async Task<WatchListItem> CreateAsync(int userId, string watchListId, string artist, string concertId)
    {
        try
        {
            var existingConcert = await _watchListItemCollection.Find(
                Builders<WatchListItem>.Filter.Eq(x => x.UserId, userId) &
                Builders<WatchListItem>.Filter.Eq(x => x.ConcertId, concertId)).FirstOrDefaultAsync();
            if (existingConcert != null)
            {
                throw new Exception($"You already have this concert ({artist} - {concertId}) in your watchlist.");
            }

            List<Concert> concerts = await _concertService.GetConcertsByArtist(artist);
            var concertNum = int.Parse(concertId);
            Concert desiredConcert = concerts.FirstOrDefault(x => x.ConcertId == concertNum)
            ?? throw new Exception($"Concert with ID {concertId} was not found for {artist}");
            var concertidToStr = desiredConcert.ConcertId.ToString();
            var watchListItem = new WatchListItem
            {
                UserId = userId,
                ConcertId = concertidToStr,
                ArtistName = string.Join(", ", desiredConcert.Performers.Select(p => p.Artist)),
                ConcertName = desiredConcert.Title,
                WatchlistId = watchListId,
                Venue = desiredConcert.Location.Name,
                Time = desiredConcert.Date,
                Price = desiredConcert.Prices.LowestPrice,
                IsAttending = false // Initially set to false
            };
            if (watchListItem is null)
            {
                throw new Exception($"Failed to create WatchList object for concert: {concertId}");
            }
            
            // var existingWatchListId = await _watchListCollection.Find(Builders<WatchList>.Filter.Eq(x => x.WatchlistId, watchListId));
            
            await _watchListItemCollection.InsertOneAsync(watchListItem);
            await AddToUserWatchListAsync(userId, watchListId, watchListItem);
            return watchListItem;
        }
        catch (MongoException ex)
        {
            // Handle errors during watch list retrieval
            throw new Exception("Failed to to create watch lists: " + ex.Message);
        }
    }
    public async Task AddToUserWatchListAsync(int userId, string watchListId, WatchListItem newWatchListItem)
    {
        try
        {
            var user = await _userService.GetAsync(userId);
            if (user is null)
            {
                throw new Exception($"User with ID {userId} not found.");
            }
            var existingWatchList = user.WatchLists.FirstOrDefault(wl => wl.WatchlistId == watchListId);
            if (existingWatchList == null)
        {
            existingWatchList = new WatchList();
            existingWatchList.WatchlistId = watchListId;
            user.WatchLists.Add(existingWatchList);
        }

        existingWatchList.Items.Add(newWatchListItem);

            /* 
            
            user {
                watchlists:[
                    objectid:
                    watchlistid:1,
                    items: [
                        {newItems},
                        {newItems},
                        {newItems}
                    ],
                    objectid:
                    watchlistid:2,
                    items: [
                        {newItems},
                        {newItems}
                    ],
                    

                ]
            }
            
            */
            await _usersCollection.ReplaceOneAsync(
                Builders<User>.Filter.Eq(x => x.UserId, userId),
                user);
        }
        catch (MongoException ex)
        {
            throw new Exception("Failed to update WatchList: " + ex.Message);
        }
    }

    // Update WatchList
    public async Task UpdateAsync(int userId, string concertId, bool attendance)
    {
        var filter = Builders<WatchListItem>.Filter.And(
        Builders<WatchListItem>.Filter.Eq(x => x.UserId, userId),
        Builders<WatchListItem>.Filter.Eq(x => x.ConcertId, concertId));
        var update = Builders<WatchListItem>.Update.Set(x => x.IsAttending, attendance);

        try
        {
            await _watchListItemCollection.UpdateOneAsync(filter, update);
            await _userService.UpdateUserWatchListAsync(userId, concertId, attendance);
        }
        catch (MongoException ex)
        {
            throw new Exception("Failed to update watch list: " + ex.Message);
        }


    }

    // Remove WatchList
    public async Task RemoveAsync(int userId, string concertId)
    {
        try
        {
            await _watchListItemCollection.DeleteOneAsync(x => x.ConcertId == concertId);
            await _userService.RemoveConcertAsync(userId, concertId);
        }
        catch (MongoException ex)
        {
            // Handle errors during watch list retrieval
            throw new Exception("Failed to to remove watch lists: " + ex.Message);
        }
    }

    internal Task<List<WatchList>> GetAllWatchListsAsync(int user_id)
    {
        throw new NotImplementedException();
    }
}
