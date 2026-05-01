public class CreateUserRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public IFormFile? Avatar { get; set; }
    public string? Legend { get; set; }
}

public class CreateUserResponseDto
{
    public string Message { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

