using api.Models;
using api.UtilityClass;
using api.UtilityClass.databaseErrors;

namespace api.Repositories.UserRepo;

public interface IUserRepository
{
    public Task<Result<SelectUser, IDatabaseError>> InsertUser(InsertUser user);
    
    public Task<List<SelectUser>> GetAllUsers();
    public Task<SelectUser?> GetUserById(Guid id);
    public Task<SelectUser?> GetUserByUsername(string username);
    public Task<SelectUser?> GetUserByEmail(string email);
    
    public Task<BasicUser?> GetUserForLogin(UserLogin user);
}
