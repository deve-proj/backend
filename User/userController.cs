using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Grpc.Core;
using System.Security.Claims;

[ApiController]
[Route("/user")]
[Authorize]
public class UserController : ControllerBase
{

    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Authenticate user and get JWT token
    /// </summary>
    /// <param name="userData">Login credentials</param>
    /// <returns>User info and JWT</returns>
    [Authorize]
    [HttpPost("check_auth")]
    public async Task<GetUserInfoDto> CheckAuth()
    {
        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        return await _userService.GetUserInfo(userId);
    }

    /// <summary>
    /// Authenticate user and get JWT token
    /// </summary>
    /// <param name="userData">Login credentials</param>
    /// <returns>User info and JWT</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Delete user by login
    /// </summary>
    /// <param name="userData"></param>
    /// <returns>Status of deleting</returns>
    [HttpPost("delete")]
    [ProducesResponseType(typeof(LoginUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Create new user
    /// </summary>
    /// <param name="userData">User credentials for creation</param>
    /// <returns>Status of creating and JWT</returns>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CreateUserResponseDto), StatusCodes.Status201Created)]
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

    /// <summary>
    ///     Refresh expired access token by refresh token
    /// </summary>
    /// <param name="data"></param>
    /// <returns>New access token</returns>
    [HttpPost]
    [Route("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RefreshTokenResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult?> RefreshToken([FromBody] RefreshTokenRequestDto data)
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