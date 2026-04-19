using api.Models;
using api.Services.DatabaseService;
using api.UtilityClass;
using api.UtilityClass.databaseErrors;
using LinqToDB.Async;
using LinqToDB;
using Npgsql;

namespace api.Repositories.UserRepo;

public class UserRepository(IDbService db, ILogger<UserRepository> logger, RedisCacheService redis) : IUserRepository
{

    public async Task<Result<SelectUser, IDatabaseError>> InsertUser(InsertUser user)
    {
        try
        {
            var usr = await db.Users
                .Value(u => u.UserName, user.UserName)
                .Value(u => u.Role, user.Role)
                .Value(u => u.Email, user.Email)
                .Value(u => u.Passcode, user.Passcode)
                .Value(u => u.IsActive, true)
                .InsertWithOutputAsync(inserted => new SelectUser
                {
                    UserId = inserted.UserId,
                    UserName = inserted.UserName,
                    Role =  inserted.Role,
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
                logger.LogError(ex, "Postgres error occurred during insert");
                return pgEx.SqlState switch
                {
                    PostgresCodes.UniqueViolation => 
                        Result<SelectUser, IDatabaseError>.Failure(new UniqueViolationError($"Email already exists: {user.Email}")),
        
                    _ => Result<SelectUser, IDatabaseError>.Failure(new GeneralDatabaseError(pgEx.MessageText, pgEx.SqlState))
                };
            }

            // If it's not a Postgres error, log and return general failure
            logger.LogError(ex, "Non-Postgres error occurred during user insertion");
            return Result<SelectUser, IDatabaseError>.Failure(new GeneralDatabaseError("An unexpected error occurred."));
        }
    }

    public async Task<ResultType<List<SelectUser>>> GetAllUsers()
    {
        var cacheVal = await redis.TryGet<List<SelectUser>>("GetAllUsers");
        if (cacheVal.Success)
        {
            logger.LogInformation("Getting all users from cache");
            return new ResultType<List<SelectUser>>()
            {
                DataFrom = "Cache",
                Data = cacheVal.Value!
            };
        }
        logger.LogInformation("cache failed");
            
        var users = await db.Users
            .Select(usr => new SelectUser{
                UserId = usr.UserId,
                UserName = usr.UserName,
                Role =  usr.Role,
                Email = usr.Email,
                IsActive = usr.IsActive,
                CreatedAt = usr.CreatedAt,
                UpdatedAt = usr.UpdatedAt,
            })
            .Where(usr => usr.IsActive)
            .OrderBy(usr => usr.CreatedAt)
            .ToListAsync();
        
        await redis.Set("GetAllUsers", users);
        logger.LogInformation("added all users to cache");
        
        return new ResultType<List<SelectUser>>()
        {
            DataFrom = "Postgres",
            Data = users
        };;
    }

    public async Task<SelectUser?> GetUserById(Guid id)
    {
        var user = await db.Users
            .Where(usr => usr.UserId == id)
            .Select(usr => new SelectUser{
                UserId = usr.UserId,
                UserName = usr.UserName,
                Email = usr.Email,
                Role =  usr.Role,
                IsActive = usr.IsActive,
                CreatedAt = usr.CreatedAt,
                UpdatedAt = usr.UpdatedAt,
            })
            .FirstOrDefaultAsync();
        
        return user;
    }

    public async Task<SelectUser?> GetUserByUsername(string username)
    {
        var user = await db.Users
            .Where(usr => usr.UserName == username)
            .Select(usr => new SelectUser{
                UserId = usr.UserId,
                UserName = usr.UserName,
                Email = usr.Email,
                Role =  usr.Role,
                IsActive = usr.IsActive,
                CreatedAt = usr.CreatedAt,
                UpdatedAt = usr.UpdatedAt,
            })
            .FirstOrDefaultAsync();
        
        return user;
    }

    public async Task<SelectUser?> GetUserByEmail(string email)
    {
        var user = await db.Users
            .Where(usr => usr.Email == email)
            .Select(usr => new SelectUser{
                UserId = usr.UserId,
                UserName = usr.UserName,
                Email = usr.Email,
                Role =  usr.Role,
                IsActive = usr.IsActive,
                CreatedAt = usr.CreatedAt,
                UpdatedAt = usr.UpdatedAt,
            })
            .FirstOrDefaultAsync();
        
        return user;
    }

    public async Task<BasicUser?> GetUserForLogin(UserLogin user)
    {
        var loginUser = await db.Users
            .Where(usr => usr.Email == user.Email && usr.Passcode == user.Passcode)
            .Select(usr => new BasicUser(){
                UserId = usr.UserId,
                UserName = usr.UserName,
                Email = usr.Email,
                Role =  usr.Role,
                IsActive = usr.IsActive,
            })
            .FirstOrDefaultAsync();
        
        return loginUser;
    }
}