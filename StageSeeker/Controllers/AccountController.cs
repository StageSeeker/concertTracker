using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Auth0.AspNetCore.Authentication;
using MongoDB.Driver;
using StageSeeker.Models;
using StageSeeker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


[ApiController]
[Route("protected")]
public class AccountController : Controller
{
  private readonly UsersService? _userService;
public AccountController(UsersService usersService) {
  _userService = usersService;
}

[HttpGet("/")]
public ActionResult Home() {
  return Ok("Log into StageSeeker using /login or protected/login endpoint");
}

//redirect routes for easy login / logout
[Route("/login")]
[HttpGet]
public IActionResult RedirectToLogin() {
  return RedirectToAction("Login", "Account");
}

[Route("/logout")]
[HttpGet]
public IActionResult RedirectToLogout() {
  return RedirectToAction("Logout", "Account");
}

[Route("/profile")]
[HttpGet]
public IActionResult RedirectToProfile() {
  return RedirectToAction("Profile", "Account");
}

[HttpGet("login")]
  public async Task Login(string returnUrl = "/swagger")
  {
    var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
      // Indicate here where Auth0 should redirect the user after a login.
      // Note that the resulting absolute Uri must be added to the
      // **Allowed Callback URLs** settings for the app.
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
    if(User.Identity == null) {
      return Unauthorized("User Identity is null");
    } else if(User.Identity.IsAuthenticated == false) {
      return Unauthorized("User Identity is not authenticated");
    }
    var name = User.Identity.Name;
    var email = User.Claims.FirstOrDefault(c=> c.Type == ClaimTypes.Email)?.Value;
    var profileImage = User.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;
    var subClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
  
    // Look into how to store password from login context
    // or disable allowing user to user credentials 
    // only allow sso  
    // Look into claims.
    if(_userService is null) {
      return StatusCode(500, "Cannot access userService");
    }
    if(name is null) {
      throw new Exception("Could not username");
    }
    var existingUser = await _userService.GetUserByName(name);
    if (existingUser is not null) {
      return Ok(new{
      Name = name,
      EmailAddress = email,
      ProfileImage = profileImage,
      Sub = subClaim
    });
    }
      var new_user = new User {
        Username = name!,
        Email = email!,
        ProfilePic = profileImage!,
        WatchLists = new List<WatchList>()
      };
        await _userService.CreateAsync(new_user);
        return Ok("User Succesfully registed with StageSeeker");
         
  }
  
  [HttpGet("logout")]
  [Authorize]
  public async Task Logout()
  {
    var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
      // Indicate here where Auth0 should redirect the user after a logout.
      // Note that the resulting absolute Uri must be added to the
      // **Allowed Logout URLs** settings for the app.
      // Points to where Auth0 should redirect after logout
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