using api.Models;
using api.Repositories.UserRepo;
using api.Services;
using api.Services.EmailService;
using api.Services.UserContextService;
using api.UtilityClass;
using api.UtilityClass.databaseErrors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class UserController(
    IUserRepository userRepo, 
    ILogger<UserController> logger, 
    TokenService tokenService,
    IUserContext userContext,
    IEmailService emailService,
    AppSettings appSettings
): ControllerBase
{    
    [HttpPost]
    public async Task<ActionResult<SelectUser>> InsertUser([FromBody] InsertUser newUser)
    {
        logger.LogInformation("Creating new user {username}", newUser.UserName);
    
        var result = await userRepo.InsertUser(newUser);

        if (result.IsSuccess)
            return CreatedAtAction(nameof(GetUserById), new { id = result.Value!.UserId }, result.Value);
        if (result.Error?.Code == PostgresCodes.UniqueViolation)
        {
            return Conflict(new { message = result.Error });
        }
        
        return BadRequest(new { message = result.Error });

    }

    [HttpPost]
    public async Task<ActionResult<string>> Login([FromBody] UserLogin user)
    {
        logger.LogInformation("User with email address {UserEmail} trying to login", user.Email);
        var usr = await userRepo.GetUserForLogin(user);
        
        if (usr != null) return Ok(tokenService.CreateToken(usr));
        
        logger.LogInformation("User login failed {UserEmail}", user.Email);
        return Unauthorized("Invalid email or password.");
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<SelectUser?>> GetUserById(Guid id)
    {
        logger.LogInformation("Getting user with id {id}", id);
        var user = await userRepo.GetUserById(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<SelectUser?>> GetUserByUserName(string userName)
    {
        logger.LogInformation("Getting user with username {username}", userName);
        var user = await userRepo.GetUserByUsername(userName);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<SelectUser?>> GetUserByEmail(string email)
    {
        logger.LogInformation("Getting user with email {email}", email);
        var user = await userRepo.GetUserByEmail(email);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<ResultType<List<SelectUser>>>> GetAllUsers()
    {
        if (userContext.User!.Role != UserRole.Admin)
        {
            return Unauthorized();
        }

        logger.LogInformation("Getting all users");
        var users = await userRepo.GetAllUsers();
        return Ok(users);
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<bool>> EmailTest()
    {
        var emailStructure = new EmailStructure
        {
            ToEmail = appSettings.Email.To,
            Body = await EmailTemplateHelper.GetTemplateAsync("WelcomeEmailTest"),
            Subject = "Welcome to API",
            IsHtml = true,
        };
        await emailService.SendEmailAsync(emailStructure);
        return Ok(true);
    }
}
