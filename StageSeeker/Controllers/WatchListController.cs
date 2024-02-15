using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
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

    [HttpGet("{id}")]
    public async Task<ActionResult<WatchList>> Get(int id)
    {
        var concert = await _watchService.GetWatchAsync(id);
        if (concert is null)
        {
            return NotFound();
        }
        return concert;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] WatchList new_watchList)
    {
        await _watchService.CreateAsync(new_watchList);
        return CreatedAtAction(nameof(Get), new { watchID = new_watchList.WatchId }, new_watchList);
    }

[HttpPatch("{id}")]
public async Task<IActionResult> Update(int id, [FromBody] Dictionary<string, object> updatedFields)
{
    var watchList = await _watchService.GetWatchAsync(id);
    if (watchList is null)
    {
        return NotFound();
    }
    
    // Call the service method with the updated fields
    await _watchService.UpdateAsync(id, updatedFields);

    return NoContent(); // 204 No Content
}


    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var concert = await _watchService.GetWatchAsync(id);
        if (concert is null)
        {
            return NotFound();
        }
        await _watchService.RemoveAsync(id);
        return StatusCode(202);
    }
}