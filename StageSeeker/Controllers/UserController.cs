using Microsoft.AspNetCore.Mvc;
using StageSeeker.Services;

namespace StageSeeker.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase {

    [HttpGet("users")]
    public IActionResult GetAllUsers() {
        var users = UserService.GetAll();
        return Ok(users);
    }
}