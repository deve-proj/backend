using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Grpc.Core;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using DeveSecurity;

[ApiController]
[Route("/user")]
[Authorize]
public class UserController : ControllerBase
{

    private readonly IUserService _userService;
    private readonly IAuth _auth;

    public UserController(IUserService userService, IAuth auth)
    {
        _userService = userService;
        _auth = auth;
    }

    [AllowAnonymous]
    [HttpGet("with-google")]
    public IActionResult GoogleLogin()
    {
        var redirectUrl = Url.Action(nameof(GoogleCallback), "User", null, Request.Scheme);
        var properties = new AuthenticationProperties{RedirectUri = redirectUrl};

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google-reply")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleCallback()
    {
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        if(!result.Succeeded)
        {
            return BadRequest("ОШибка входа через Google");
        }

        var email = result.Principal.FindFirst("email")?.Value;
        var name = result.Principal.FindFirst("name")?.Value;

        // var token = _auth.GenerateAccessToken(new GetUserDto
        // {
        //     UserId = Guid.NewGuid(),
        //     Name = name!,
        //     Login = email!
        // });

        await _userService.LoginOrRegist(email!, name!);

        string frontendUrl = "http://localhost:5173";

        return Redirect($"{frontendUrl}");
    }

    
    [Authorize]
    [HttpPost("check_auth")]
    public async Task<GetUserInfoDto> CheckAuth()
    {
        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        return await _userService.GetUserInfo(userId);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult> Login([FromBody] LoginUserDto userData)
    {
        Console.Write(userData.password + " " + userData.login + "\n");

        var result = await _userService.Login(userData.login, userData.password);

        if(result != null)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Domain = "localhost",
                Expires = DateTime.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("accessToken", result.AccessToken, cookieOptions);
            Response.Cookies.Append("refreshToken", result.RefreshToken, cookieOptions);

            return Ok(new CreateUserResponseDto
            {
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken
            });
        }

        return Unauthorized();
    }

    [HttpPost("delete")]
    public async Task<IActionResult> DeleteUser([FromBody] DeleteUserDto userData)
    {
        Console.WriteLine(userData);
        try
        {
            await _userService.DeleteUser(userData.Login);

            return Ok("User was successfully deleted!");
        }

        catch
        {
            return BadRequest("Failed to delete user");
        }
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateUser([FromForm] CreateUserRequestDto userData)
    {
        try
        {
            var result = await _userService.CreateUser(userData);
            
            return Ok(new
                CreateUserResponseDto{
                    Message = "User was successfully created!",
                    AccessToken = result!.AccessToken,
                    RefreshToken = result!.RefreshToken
                }
            );
        }
        
        catch(Exception e)
        {
            return BadRequest(new
            {
                Message = "Failed to create user: " + e.Message
            });
        }
    }

    [HttpPost]
    [Route("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult?> GetRefreshToken([FromBody] RefreshTokenRequestDto data)
    {
        try
        {
            return Ok(new RefreshTokenResponseDto()
            {
                AccessToken = (await _userService.RefreshAccessToken(data))!.AccessToken
            });
        }

        catch(Exception e)
        {
            return BadRequest(new
            {
                
                Message = "Failed to update token: " + e.Message
                
            });
        }
    }
}