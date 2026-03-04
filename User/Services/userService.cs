using DeveSecurity;
using Microsoft.AspNetCore.Http.HttpResults;

public class ICreateUser
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public interface IUserService
{
    public Task<ICreateUser?> CreateUser(CreateUserRequestDto userData);
    public Task<User?> GetUser(string login, string password);
    public Task<bool> UpdateUserLogin(string newLogin);
    public Task<bool> UpdateUserName(string newName);
    public Task<bool> UpdateUserPassword(string newPassword);
    public Task<bool> DeleteUser(string login);
    public Task<RefreshTokenResponseDto?> RefreshAccessToken(RefreshTokenRequestDto data);

}

public class UserService : IUserService
{

    private readonly IUserRepo _userRepo;

    public UserService(IUserRepo userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<ICreateUser?> CreateUser(CreateUserRequestDto userData)
    {
        
        try
        {
            Guid userId = Guid.NewGuid();

            string AccessToken = Auth.GenerateAccessToken(new GetUserDto{Name = userData.Name, Login = userData.Login, UserId = userId});
            string RefreshToken = Auth.GenerateRefreshToken(new GetUserDto{Name = userData.Name, Login = userData.Login, UserId = userId});

            await _userRepo.CreateUser(new User()
                {
                    Name = userData.Name,
                    Login = userData.Login,
                    Password = BCrypt.Net.BCrypt.HashPassword(userData.Password),
                    UserId = userId,
                    RefreshToken = Auth.HashToken(RefreshToken)
                }
            );

            return new ICreateUser{AccessToken = AccessToken, RefreshToken = RefreshToken};
        }

        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<User?> GetUser(string login, string password)
    {
        return await _userRepo.GetUser(login, password);
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

            Guid userId = Auth.DecodeToken(data.RefreshToken).UserId;
            
            string originTokenHash = await _userRepo.GetRefreshTokenHashByUserId(userId);

            if(Auth.VerifyTokenHashs(data.RefreshToken, originTokenHash))
            {

                string tokenHash = Auth.HashToken(data.RefreshToken);
                User user = (await _userRepo.GetUserByRefreshToken(originTokenHash))!;
                
                Console.WriteLine(user);


                string AccessToken = Auth.GenerateAccessToken(new GetUserDto()
                    {
                        UserId = userId,
                        Name = user!.Name,
                        Login = user!.Login
                    }
                );

                return new RefreshTokenResponseDto(){AccessToken = AccessToken};
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
}