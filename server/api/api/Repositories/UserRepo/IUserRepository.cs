using api.Models;
using Microsoft.AspNetCore.Identity;

namespace api.Repositories.UserRepo;

public interface IUserRepository
{
    public Task<SelectUser?> InsertUser(InsertUser user);
    
    public Task<List<SelectUser>> GetAllUsers();
    public Task<SelectUser?> GetUserById(Guid id);
    public Task<SelectUser?> GetUserByUsername(string username);
    public Task<SelectUser?> GetUserByEmail(string email);
    
    public Task<BasicUser?> GetUserForLogin(UserLogin user);
}
