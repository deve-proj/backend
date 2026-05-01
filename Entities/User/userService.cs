using DeveSecurity;

public class ICreateUser
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public interface IUserService
{
    public Task<ICreateUser?> CreateUser(CreateUserRequestDto userData);
    public Task<ICreateUser?> Login(string login, string password);
    public Task<ICreateUser?> LoginOrRegist(string name, string email);
    public Task<bool> UpdateUserLogin(string newLogin);
    public Task<bool> UpdateUserName(string newName);
    public Task<bool> UpdateUserPassword(string newPassword);
    public Task<bool> DeleteUser(string login);
    public Task<RefreshTokenResponseDto?> RefreshAccessToken(RefreshTokenRequestDto data);
    public Task<GetUserInfoDto> GetUserInfo(string userId);
    public Task<List<GetUserInfoDto>> GetUsersInfo(string[] userIds);

}

public class UserService : IUserService
{

    private readonly IUserRepo _userRepo;
    public readonly IDeveMinioClient _minioClient;
    private readonly IAuth _authService;

    public UserService(IUserRepo userRepo, IDeveMinioClient minioClient, IAuth authService)
    {
        _userRepo = userRepo;
        _minioClient = minioClient;
        _authService = authService;
    }

    public async Task<ICreateUser?> LoginOrRegist(string email, string name)
    {
        var result = await _userRepo.GetUserByEmailAndName(email, name);

        if(result != null)
        {
            return await Login(result.Login, result.Password);
        }
        else
        {
            return await CreateUser(new CreateUserRequestDto
            {
                Name = name,
                Email = email,

            });
        }
    }

    public async Task<ICreateUser?> CreateUser(CreateUserRequestDto userData)
    {
        
        try
        {
            Guid userId = Guid.NewGuid();

            string key = "";

            string AccessToken = _authService.GenerateAccessToken(new GetUserDto{Name = userData.Name, Login = userData.Login, UserId = userId});
            string RefreshToken = _authService.GenerateRefreshToken(new GetUserDto{Name = userData.Name, Login = userData.Login, UserId = userId});

            if(userData.Avatar != null)
            {
                using var stream = userData.Avatar.OpenReadStream();
                key = $"{userId}/avatar/avatar.png";

                await _minioClient.PutObject(stream, key, userData.Avatar.ContentType, userData.Avatar.Length);
            }

            await _userRepo.CreateUser(new User()
                {
                    Name = userData.Name,
                    Login = userData.Login,
                    Password = BCrypt.Net.BCrypt.HashPassword(userData.Password),
                    AvatarUrl = $"http://localhost:9000/users/{key}",
                    UserId = userId,
                    RefreshToken = _authService.HashToken(RefreshToken)
                }
            );

            return new ICreateUser{AccessToken = AccessToken, RefreshToken = RefreshToken};
        }

        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<ICreateUser?> Login(string login, string password)
    {
        User? user = await _userRepo.GetUserByLogin(login);

        if(user == null)
        {
            return null;
        }
        else
        {
            if(BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                string AccessToken = _authService.GenerateAccessToken(new GetUserDto{Name = user.Name, Login = user.Login, UserId = user.UserId});
                string RefreshToken = _authService.GenerateRefreshToken(new GetUserDto{Name = user.Name, Login = user.Login, UserId = user.UserId});

                await _userRepo.UpdateRefreshToken(_authService.HashToken(RefreshToken), user.UserId);

                return new ICreateUser{AccessToken = AccessToken, RefreshToken = RefreshToken};
            }
            else
            {
                return null;
            }
        }
    }

    public async Task<bool> DeleteUser(string login)
    {
        return await _userRepo.DeleteUser(login);
    }

    public async Task<bool> UpdateUserLogin(string newLogin)
    {
        return true;
    }

    public async Task<bool> UpdateUserName(string newName)
    {
        return true;
    }

    public async Task<bool> UpdateUserPassword(string newPassword)
    {
        return true;
    }

    public async Task<RefreshTokenResponseDto?> RefreshAccessToken(RefreshTokenRequestDto data)
    {
        try
        {

            Guid userId = _authService.DecodeToken(data.RefreshToken).UserId;
            
            string originTokenHash = await _userRepo.GetRefreshTokenHashByUserId(userId);

            if(_authService.VerifyTokenHashs(data.RefreshToken, originTokenHash))
            {
                User user = (await _userRepo.GetUserByRefreshToken(originTokenHash))!;


                string AccessToken = _authService.GenerateAccessToken(new GetUserDto()
                    {
                        UserId = userId,
                        Name = user!.Name,
                        Login = user!.Login
                    }
                );

                string RefreshToken = _authService.GenerateRefreshToken(new GetUserDto()
                    {
                        UserId = userId,
                        Name = user!.Name,
                        Login = user!.Login
                    }
                );

                await _userRepo.UpdateRefreshToken(_authService.HashToken(RefreshToken), userId);

                return new RefreshTokenResponseDto(){AccessToken = AccessToken, RefreshToken = RefreshToken};
            }

            else
            {
                throw new Exception("Invalid refresh token");
            }
        }

        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<GetUserInfoDto> GetUserInfo(string userId)
    {
        try
        {
            var user = await _userRepo.GetUserByUserId(userId);

            return new GetUserInfoDto
            {
                UserId = user!.UserId.ToString(),
                Name = user!.Name,
                Login = user!.Login,
                Legend = user!.Legend,
                Avatar = user!.AvatarUrl,
                ReputationScore = user!.ReputationScore
            };

        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<List<GetUserInfoDto>> GetUsersInfo(string[] userIds)
    {
        try
        {
            var result = await _userRepo.GetUsersByIds(userIds);

            List<GetUserInfoDto> users = [];

            foreach(var user in result!)
            {
                users.Add(new GetUserInfoDto
                {
                    UserId = user!.UserId.ToString(),
                    Name = user!.Name,
                    Login = user!.Login,
                    Legend = user!.Legend,
                    Avatar = user!.AvatarUrl,
                    ReputationScore = user!.ReputationScore
                });
            }

            return users;

        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}