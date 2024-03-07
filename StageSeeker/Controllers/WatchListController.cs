using Microsoft.AspNetCore.Mvc;
using StageSeeker.Models;
using StageSeeker.Services;

namespace StageSeeker.Controllers;

[ApiController]
[Route("watchlist")]
public class WatchListController : ControllerBase
{
    private readonly WatchListService _watchService;
    public WatchListController(WatchListService watchService)
    {
        _watchService = watchService;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<List<WatchListItem>>> Get(int userId)
    {
        try
        {
            var userWatchList = await _watchService.GetUserWatchAsync(userId);
            if (userWatchList is null)
            {
                return NotFound();
            }
            return userWatchList;
        }
        catch (Exception ex)
        {
            // Handle errors 
            Console.Error.WriteLine("Error fetching watchlists: " + ex.Message);
            return StatusCode(500, "Failed to fetch watchlists: " + ex.Message);
        }
    }

    [HttpPost("{userId}/{watchListId}/{artist}/{concertId}")]
    public async Task<IActionResult> Post(int userId, string watchListId, string artist, string concertId)
    {
        try
        {
            var watchList = await _watchService.CreateAsync(userId, watchListId, artist, concertId);
            if (watchList is null)
            {
                throw new Exception("Failed to create WatchList");
            }
            return StatusCode(StatusCodes.Status201Created, watchList);
        }
        catch (Exception ex)
        {
            // Handle errors gracefully
            // Console.Error.WriteLine("Error creating watch list: " + ex.Message);
            return StatusCode(500, "Error: " + ex.Message);
        }
    }


    // Updates the attendence 

    [HttpPatch("{userId}/{concertId}")]
    public async Task<IActionResult> Update(int userId, string watchlistId, string concertId, [FromBody] bool updatedValues)
    {
        try
        {
            var watchList = await _watchService.GetOneUserConcertAsync(userId, concertId);

            if (watchList is null)
            {
                return NotFound();
            }

            // Call the service method with the updated fields
            await _watchService.UpdateAsync(userId, watchlistId, concertId, updatedValues);

            return NoContent(); // 204 No Content
        }
        catch (Exception ex)
        {
            // Handle errors 
            Console.Error.WriteLine("Error updating watch list: " + ex.Message);
            return StatusCode(500, "Failed to update watch list: " + ex.Message);
        }
    }


    // Delete entire Watchlist
    [HttpDelete("{userId}/{watchlistId}")]
    public async Task<IActionResult> Delete(int userId, string watchlistId)
    {
        try
        {
            var concert = await _watchService.GetOneUserConcertAsync(userId, watchlistId);
            if (concert is null)
            {
                throw new Exception($"Could not find concert on watch list with ID: {watchlistId}");
            }
            await _watchService.RemoveAsync(userId, watchlistId);
            return StatusCode(202);
        }
        catch (Exception ex)
        {
            return NotFound("Failed to delete concert: " + ex.Message);
        }
    }


    // Delete Concert from Items array
    [HttpDelete("{userId}/{watchlistId}/{concertId}")]
    public async Task<IActionResult> Delete(int userId, string watchlistId, string concertId)
    {
        try
        {
            // Call the service method to remove the concert from the watchlist's items array
            await _watchService.RemoveConcertFromWatchListAsync(userId, watchlistId, concertId);


            return StatusCode(202); // 202 Accepted
        }
        catch (Exception ex)
        {
            return NotFound("Failed to delete concert: " + ex.Message);
        }
    }



}