using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GetUserDto?>> GetUser([FromBody] LoginUserDto userData)
    {
        var result = await _userService.GetUser(userData.login, userData.password);

        if(result != null)
        {
            return Ok(new GetUserDto(){Name = result.Name});
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
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto userData)
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