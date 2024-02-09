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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, WatchList update_WatchList)
    {
        var concert = await _watchService.GetWatchAsync(id);
        if (concert is null)
        {
            return NotFound();
        }

        // Extract attendance status from WatchList
        bool isAttending = update_WatchList.IsAttending;
        await _watchService.UpdateAsync(id, isAttending);
        return StatusCode(201);
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