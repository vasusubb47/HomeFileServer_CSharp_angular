using api.Models;
using api.Repositories.UserRepo;
using api.Services;
using api.UtilityClass.databaseErrors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class UserController(IUserRepository userRepo, ILogger<UserController> logger, TokenService tokenService): ControllerBase
{
    private readonly IUserRepository _userRepo = userRepo;
    private readonly ILogger<UserController> _logger = logger;
    private readonly TokenService _tokenService = tokenService;
    
    [HttpPost]
    public async Task<ActionResult<SelectUser>> InsertUser([FromBody] InsertUser newUser)
    {
        _logger.LogInformation("Creating new user {username}", newUser.UserName);
    
        var result = await _userRepo.InsertUser(newUser);

        if (!result.IsSuccess)
        {
            if (result.Error?.Code == PostgresCodes.UniqueViolation)
            {
                return Conflict(new { message = result.Error });
            }
        
            return BadRequest(new { message = result.Error });
        }

        return CreatedAtAction(nameof(GetUserById), new { id = result.Value!.UserId }, result.Value);
    }

    [HttpPost]
    public async Task<ActionResult<string>> Login([FromBody] UserLogin user)
    {
        _logger.LogInformation("User with email address {UserEmail} trying to login", user.Email);
        var usr = await _userRepo.GetUserForLogin(user);
        if (usr == null)
        {
            _logger.LogInformation("User login failed {UserEmail}", user.Email);
            return Unauthorized("Invalid email or password.");
        }
        return Ok(_tokenService.CreateToken(usr));
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<SelectUser?>> GetUserById(Guid id)
    {
        _logger.LogInformation("Getting user with id {id}", id);
        var user = await _userRepo.GetUserById(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<SelectUser?>> GetUserByUserName(string userName)
    {
        _logger.LogInformation("Getting user with username {username}", userName);
        var user = await _userRepo.GetUserByUsername(userName);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<SelectUser?>> GetUserByEmail(string email)
    {
        _logger.LogInformation("Getting user with email {email}", email);
        var user = await _userRepo.GetUserByEmail(email);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<SelectUser?>> GetAllUsers()
    {
        _logger.LogInformation("Getting all users");
        var users = await _userRepo.GetAllUsers();
        return Ok(users);
    }
}
