using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.Cookies;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Auth0.ManagementApi;
using MongoDB.Driver;
using StageSeeker.Models;
using StageSeeker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using StageSeeker.Services;

[ApiController]
[Route("protected")]
public class AccountController : Controller
{
  private readonly IConfiguration? _configuration;
  private readonly UsersService? _userService;
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

    var name = User?.Identity?.Name;
    var email = User?.Claims.FirstOrDefault(c=>c.Type == ClaimTypes.Email)?.Value;

    var newUser = new User {
      UserId = 0,
      Username = email,
      Password ="password123",
      WatchList = new WatchList()
    };
    try {
      await _userService.CreateAsync(newUser);
    } catch ( Exception ex) {
      Console.Write(ex.Message);
    }
  }
  
  [HttpGet("profile")]
  [Authorize]
  public IActionResult Profile()
  {
    if(User.Identity == null) {
      return Unauthorized("User Identity is null");
    } else if(User.Identity.IsAuthenticated == false) {
      return Unauthorized("User Identity is not authenticated");
    }
      var name = User.Identity.Name;
      var email = User.Claims.FirstOrDefault(c=> c.Type == ClaimTypes.Email)?.Value;
    var profileImage = User.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;
    return Ok(new{
      Name = name,
      EmailAddress = email,
      ProfileImage = profileImage
    });    
  }
  
  [HttpGet("logout")]
  [Authorize]
  public async Task<IActionResult> Logout()
  {
    var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
      // Indicate here where Auth0 should redirect the user after a logout.
      // Note that the resulting absolute Uri must be added to the
      // **Allowed Logout URLs** settings for the app.
      // Points to where Auth0 should redirect after logout
      .WithRedirectUri("/logout")
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
   return Ok("Thank you for using StageSeeker"); 
  }
}