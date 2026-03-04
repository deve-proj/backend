using Microsoft.EntityFrameworkCore;

public interface IUserRepo
{
    Task<User?> GetUser(string login, string password);
    Task CreateUser(User UserData);
    Task<bool> DeleteUser(string login);
    Task<string> GetRefreshTokenHashByUserId(Guid userId);
    Task<User?> GetUserByRefreshToken(string RefreshToken);
    
}

public class UserRepo : IUserRepo
{
    private readonly ApplicationDbContext _context;

    public UserRepo(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByRefreshToken(string RefreshToken)
    {
        return await _context.Users.Where(e => e.RefreshToken == RefreshToken).FirstOrDefaultAsync();
    }

    public async Task<User?> GetUser(string login, string password)
    {
        return await _context.Users.Where(e => e.Login == login).Where(e => e.Password == password).FirstOrDefaultAsync();
    }

    public async Task CreateUser(User UserData)
    {
        try
        {
            await _context.Users.AddAsync(UserData);
            await _context.SaveChangesAsync();
        }

        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task<bool> DeleteUser(string login)
    {
        try
        {
            await _context.Users.Where(e => e.Login == login).ExecuteDeleteAsync();

            return true;
        }

        catch
        {
            return false;
        }
    }

    public async Task<string> GetRefreshTokenHashByUserId(Guid userId)
    {
        
        try
        {
            return (await _context.Users.Where(e => e.UserId == userId).FirstAsync()).RefreshToken;
        }

        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}