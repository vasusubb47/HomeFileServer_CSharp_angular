using api.Models;
using api.Services.DatabaseService;
using LinqToDB.Async;
using LinqToDB;

namespace api.Repositories.UserRepo;

public class UserRepository(IDbService db) : IUserRepository
{
    private readonly IDbService _db = db;

    public async Task<SelectUser?> InsertUser(InsertUser user)
    {
        // We use the table directly to perform an Insert that returns the object
        var usr = await _db.Users
            .Value(u => u.UserName, user.UserName)
            .Value(u => u.Email,    user.Email)
            .Value(u => u.Passcode, user.Passcode)
            .Value(u => u.IsActive, true)
            // This is the specific method that handles the RETURNING clause in Postgres
            .InsertWithOutputAsync(inserted => new SelectUser
            {
                UserId = inserted.UserId,
                UserName = inserted.UserName,
                Email = inserted.Email,
                CreatedAt = inserted.CreatedAt,
                IsActive = inserted.IsActive
            });

        return usr;
    }

    public async Task<List<SelectUser>> GetAllUsers()
    {
        var users = await _db.Users
            .Select(usr => new SelectUser{
                UserId = usr.UserId,
                UserName = usr.UserName,
                Email = usr.Email,
                IsActive = usr.IsActive,
                CreatedAt = usr.CreatedAt,
                UpdatedAt = usr.UpdatedAt,
            })
            .Where(usr => usr.IsActive)
            .OrderBy(usr => usr.CreatedAt)
            .ToListAsync();
        
        return users;
    }

    public async Task<SelectUser?> GetUserById(Guid id)
    {
        var user = await _db.Users
            .Where(usr => usr.UserId == id)
            .Select(usr => new SelectUser{
                UserId = usr.UserId,
                UserName = usr.UserName,
                Email = usr.Email,
                IsActive = usr.IsActive,
                CreatedAt = usr.CreatedAt,
                UpdatedAt = usr.UpdatedAt,
            })
            .FirstOrDefaultAsync();
        
        return user;
    }

    public async Task<SelectUser?> GetUserByUsername(string username)
    {
        var user = await _db.Users
            .Where(usr => usr.UserName == username)
            .Select(usr => new SelectUser{
                UserId = usr.UserId,
                UserName = usr.UserName,
                Email = usr.Email,
                IsActive = usr.IsActive,
                CreatedAt = usr.CreatedAt,
                UpdatedAt = usr.UpdatedAt,
            })
            .FirstOrDefaultAsync();
        
        return user;
    }

    public async Task<SelectUser?> GetUserByEmail(string email)
    {
        var user = await _db.Users
            .Where(usr => usr.Email == email)
            .Select(usr => new SelectUser{
                UserId = usr.UserId,
                UserName = usr.UserName,
                Email = usr.Email,
                IsActive = usr.IsActive,
                CreatedAt = usr.CreatedAt,
                UpdatedAt = usr.UpdatedAt,
            })
            .FirstOrDefaultAsync();
        
        return user;
    }

    public async Task<BasicUser?> GetUserForLogin(UserLogin user)
    {
        var loginUser = await _db.Users
            .Where(usr => usr.Email == user.Email && usr.Passcode == user.Passcode)
            .Select(usr => new BasicUser(){
                UserId = usr.UserId,
                UserName = usr.UserName,
                Email = usr.Email,
                IsActive = usr.IsActive,
            })
            .FirstOrDefaultAsync();
        
        return loginUser;
    }
}