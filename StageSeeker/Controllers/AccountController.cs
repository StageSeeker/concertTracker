using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Auth0.AspNetCore.Authentication;
using MongoDB.Driver;
using StageSeeker.Models;
using StageSeeker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


public class AccountController : ControllerBase
{
  private readonly UsersService? _userService;
  public AccountController(UsersService usersService)
  {
    _userService = usersService;
  }

  [HttpGet("/")]
  public ActionResult Home()
  {
    return Ok("Log into StageSeeker using /login");
  }

  // Helper Method to return user info from auth0
  private (string name, string email, string profileImage) GetUserInfo()
  {
    try
    {
      if (User.Identity == null || !User.Identity.IsAuthenticated)
      {
        throw new UnauthorizedAccessException("User is not authenticated");
      }
      var name = User.Identity.Name;
      var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
      var profileImage = User.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;
      return (name!, email!, profileImage!);
    }
    catch (Exception ex)
    {
      throw new Exception("Error: " + ex.Message);
    }
  }

  [HttpGet("login")]
  public async Task Login(string returnUrl = "/profile")
  {
    var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
      .WithRedirectUri(returnUrl)
      .Build();

    await HttpContext.ChallengeAsync(
      Auth0Constants.AuthenticationScheme,
      authenticationProperties
    );
  }

  [HttpGet("profile")]
  [Authorize]
  public async Task<IActionResult> Profile()
  {
    var (name, email, profileImage) = GetUserInfo();

    if (_userService is null)
    {
      return StatusCode(500, "Cannot access userService");
    }
    if (name is null)
    {
      throw new Exception("Could not username");
    }

    try
    {
      var existingUser = await _userService.GetUserByName(name);
      if (existingUser is not null)
      {
        return Ok(new
        {
          Name = name,
          EmailAddress = email,
          ProfileImage = profileImage
        });
      }
      else
      {
        return RedirectToAction("CreateProfile");
      }
    }
    catch (Exception ex)
    {
      throw new Exception("Error: " + ex.Message);
    }
  }

  [HttpGet("create-profile")]
  [Authorize]
  public async Task<IActionResult> CreateProfile()
  {
    try
    {
      var (name, email, profileImage) = GetUserInfo();
      if (_userService is null)
      {
        return StatusCode(500, "Cannot access userService");
      }
      if (name is null)
      {
        throw new Exception("Could not username");
      }
      var new_user = new User
      {
        Username = name!,
        Email = email!,
        ProfilePic = profileImage!,
        WatchList = []
      };
      await _userService.CreateAsync(new_user);
      return Ok($@"User {new_user.Username} successfully registered with StageSeeker. 
Please go to /profile to view your stored account details.
Go to /swagger to access the StageSeeker Api");
    }
    catch (Exception ex)
    {
      throw new Exception("Error: " + ex.Message);
    }
  }

  [HttpGet("logout")]
  [Authorize]
  public async Task Logout()
  {
    var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
      .WithRedirectUri("/")
      .Build();

    // Logout from Auth0
    await HttpContext.SignOutAsync(
      Auth0Constants.AuthenticationScheme,
      authenticationProperties
    );
    // Logout from the application
    await HttpContext.SignOutAsync(
      CookieAuthenticationDefaults.AuthenticationScheme
    );
  }
}