using Microsoft.AspNetCore.Mvc;
using StageSeeker.Models;
using StageSeeker.Services;

namespace StageSeeker.Services;

[ApiController]
[Route("concerts")]

public class ConcertController : ControllerBase {
    private readonly ConcertService _concertService;
     public ConcertController(ConcertService concertService) => _concertService = concertService;
    
    [HttpGet("{artist}")]
    public async Task<ActionResult<Concert>> Get(string artist) 
    {
        var concerts = await _concertService.GetConcertsByArtist(artist);
        if (concerts is null) {
            return NotFound();
        } 
        return Ok(concerts);
    }
}