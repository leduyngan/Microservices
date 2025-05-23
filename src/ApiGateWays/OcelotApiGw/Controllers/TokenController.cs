using Contracts.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Identity;

namespace OcelotApiGw.Controllers;

[ApiController]
[Route("[controller]")]
public class TokenController : ControllerBase
{
    private readonly ITokenService _tokenService;

    public TokenController(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetToken()
    {
        var result = _tokenService.GetToken(new TokenRequest());
        return Ok(result);
    }
}