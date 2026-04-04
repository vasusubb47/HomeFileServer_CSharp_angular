using api.Models;
using api.Services.DatabaseService;
using api.UtilityClass;
using api.UtilityClass.databaseErrors;
using LinqToDB.Async;
using LinqToDB;
using Microsoft.Extensions.Caching.Distributed;
using Npgsql;

namespace api.Repositories.UserRepo;

public class UserRepository(IDbService db, ILogger<UserRepository> logger, RedisCacheService redis) : IUserRepository
{
    private readonly IDbService _db = db;
    private readonly ILogger<UserRepository> _logger = logger;
    private readonly RedisCacheService _redis = redis;

    public async Task<Result<SelectUser, IDatabaseError>> InsertUser(InsertUser user)
    {
        try
        {
            var usr = await _db.Users
                .Value(u => u.UserName, user.UserName)
                .Value(u => u.Email,    user.Email)
                .Value(u => u.Passcode, user.Passcode)
                .Value(u => u.IsActive, true)
                .InsertWithOutputAsync(inserted => new SelectUser
                {
                    UserId = inserted.UserId,
                    UserName = inserted.UserName,
                    Email = inserted.Email,
                    CreatedAt = inserted.CreatedAt,
                    IsActive = inserted.IsActive
                });

            return Result<SelectUser, IDatabaseError>.Success(usr);
        }
        catch (Exception ex) // Catch base Exception to be safe
        {
            // Search the entire exception tree for a PostgresException
            if (ex.GetBaseException() is PostgresException pgEx || 
                (ex.InnerException is PostgresException innerPgEx && (pgEx = innerPgEx) != null))
            {
                _logger.LogError(ex, "Postgres error occurred during insert");
                return pgEx.SqlState switch
                {
                    PostgresCodes.UniqueViolation => 
                        Result<SelectUser, IDatabaseError>.Failure(new UniqueViolationError($"Email already exists: {user.Email}")),
        
                    _ => Result<SelectUser, IDatabaseError>.Failure(new GeneralDatabaseError(pgEx.MessageText, pgEx.SqlState))
                };
            }

            // If it's not a Postgres error, log and return general failure
            _logger.LogError(ex, "Non-Postgres error occurred during user insertion");
            return Result<SelectUser, IDatabaseError>.Failure(new GeneralDatabaseError("An unexpected error occurred."));
        }
    }

    public async Task<List<SelectUser>> GetAllUsers()
    {
        var cacheVal = await _redis.TryGet<List<SelectUser>>("GetAllUsers");
        if (cacheVal.Success)
        {
            _logger.LogInformation("Getting all users from cache");
            return cacheVal.Value!;
        }
        _logger.LogInformation("cache failed");
            
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
        
        await _redis.Set("GetAllUsers", users);
        _logger.LogInformation("added all users to cache");
        
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