using StageSeeker.Models;
using StageSeeker.Services;
using Microsoft.AspNetCore.Mvc;


namespace StageSeeker.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase 
{
    private readonly UsersService _usersService;

    public UsersController(UsersService usersService) => _usersService = usersService;

    [HttpGet]
    public async Task<List<User>> Get() => await _usersService.GetAsync();

}