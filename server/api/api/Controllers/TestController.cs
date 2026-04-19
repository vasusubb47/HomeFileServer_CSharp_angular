using api.Services.UserContextService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
[Authorize]
public class TestController(IUserContext userContext) : ControllerBase
{
    private readonly IUserContext _userContext = userContext;

    [HttpGet]
    public async Task<ActionResult> GetTokenInfo()
    {
        var usr = _userContext;
        return Ok(usr);
    }
}
