using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

public interface IUserRepo
{
    Task<User?> GetUserByLogin(string login);
    Task<User?> GetUserByUserId(string userId);
    Task<User?> GetUserByEmailAndName(string email, string name);
    Task<List<User>?> GetUsersByIds(string[] userIds);
    Task CreateUser(User UserData);
    Task<bool> DeleteUser(string login);
    Task UpdateRefreshToken(string refreshToken, Guid userId);
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

    public async Task<User?> GetUserByEmailAndName(string email, string name)
    {
        return await _context.Users.Where(e => e.Name == name && e.Email == email).FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByRefreshToken(string RefreshToken)
    {
        return await _context.Users.Where(e => e.RefreshToken == RefreshToken).FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByLogin(string login)
    {
        return await _context.Users.Where(e => e.Login == login).FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByUserId(string userId)
    {
        return await _context.Users.Where(e => e.UserId == Guid.Parse(userId)).FirstOrDefaultAsync();
    }

    public async Task<List<User>?> GetUsersByIds(string[] userIds)
    {
        return await _context.Users.Where(e => userIds.Contains(e.UserId.ToString())).ToListAsync();
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

    public async Task UpdateRefreshToken(string refreshToken, Guid userId)
    {
        await _context.Users.Where(e => e.UserId == userId).ExecuteUpdateAsync(setters => setters.SetProperty(u => u.RefreshToken, refreshToken));
    }
}