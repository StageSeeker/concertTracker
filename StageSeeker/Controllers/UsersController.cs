using StageSeeker.Models;
using StageSeeker.Services;
using Microsoft.AspNetCore.Mvc;


namespace StageSeeker.Controllers;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase 
{
    private readonly UsersService _usersService;

    public UsersController(UsersService usersService) => _usersService = usersService;

    // GET All users
    [HttpGet]
    public async Task<List<User>> Get() => await _usersService.GetAsync();

    // GET one user
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> Get(int id) 
    {
        var user = await _usersService.GetAsync(id);
        if (user is null) {
            return NotFound();
        } 
        return user;
    }

    // POST a user
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] User new_user)
    {
        await _usersService.CreateAsync(new_user);
        return CreatedAtAction(nameof(Get), new { userId = new_user.UserId }, new_user);
    }
    
    // DELETE a user
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete (int id)
    {
        var user = await _usersService.GetAsync(id);
        if (user is null) {
            return NotFound();
        }
        await _usersService.RemoveAsync(id);
        return StatusCode(202);
    }

   
}