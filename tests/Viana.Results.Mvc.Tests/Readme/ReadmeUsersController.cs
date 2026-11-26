using Microsoft.AspNetCore.Mvc;
using Viana.Results.Mvc;

namespace Viana.Results.Mvc.Tests.Readme;

/// <summary>Mirror of the "Wrapping a Result in an ActionResult" and filter snippets of the Mvc README.</summary>
[ApiController]
[Route("api/[controller]")]
public class ReadmeUsersController : ControllerBase
{
    private readonly IReadmeUserService _userService;

    public ReadmeUsersController(IReadmeUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var result = await _userService.GetUserByIdAsync(id);

        // Wraps the result in an ActionResult that writes its status and body
        return new VianaResultAction(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(ReadmeCreateUserDto dto)
    {
        var result = await _userService.CreateAsync(dto);
        return new VianaResultAction(result);
    }

    // With the global filter, return IResult directly
    [HttpGet]
    public async Task<IResult> GetUsers()
    {
        var users = await _userService.GetAllAsync();
        return users; // Will be automatically formatted
    }
}
