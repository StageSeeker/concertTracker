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

    [HttpGet]
    public async Task<List<WatchList>> Get()
    {
        return await _watchService.GetWatchAsync();

    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<List<WatchList>>> Get(int userId)
    {
        var userWatchList = await _watchService.GetUserWatchAsync(userId);
        if (userWatchList is null)
        {
            return NotFound();
        }
        return userWatchList;
    }

    [HttpPost("{userId}/{artist}/{concertId}")]
    public async Task<IActionResult> Post(int userId, string artist, int concertId)
    {
        try
        {
            await _watchService.CreateAsync(userId, artist, concertId);
            return CreatedAtAction(nameof(Post), new { user = userId }, null);
        }
        catch (Exception ex)
        {
            // Handle errors gracefully
            Console.Error.WriteLine("Error creating watch list: " + ex.Message);
            return StatusCode(500, "Failed to create watch list: " + ex.Message);
        }
    }

    [HttpPatch("{userId}/{concertId}")]
    public async Task<IActionResult> Update(int userId, int concertId, [FromBody] bool updatedValues)
    {
        var watchList = await _watchService.GetOneUserConcertAsync(userId, concertId);

        if (watchList is null)
        {
            return NotFound();
        }

        // Call the service method with the updated fields
        await _watchService.UpdateAsync(userId, concertId, updatedValues);

        return NoContent(); // 204 No Content
    }

    [HttpDelete("{userId}/{concertId}")]
    public async Task<IActionResult> Delete(int userId, int concertId)
    {
        try {
            var concert = await _watchService.GetOneUserConcertAsync(userId, concertId);
        if (concert is null)
        {
            throw new Exception($"Could not find concert on watch list with ID: {concertId}");
        }
        await _watchService.RemoveAsync(userId,concertId);
        return StatusCode(202);
        } catch (Exception ex) {
            return NotFound("Failed to delete concert: " + ex.Message);
        }
    }
}