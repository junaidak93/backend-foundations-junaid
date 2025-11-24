using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using NotesApi.Helpers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        var user = await _authService.Register(req.Username, req.Password);
        if (user == null) return BadRequest(new { error = "User already exists" });

        return Ok(new { message = "Registered successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var userWithToken = await _authService.Login(req.Username, req.Password);
        
        if (!userWithToken.HasValue) 
            return Unauthorized();
        
        return Ok(new AuthResponse { 
            Username = userWithToken.Value.user?.Username ?? "", 
            Token = userWithToken.Value.token ?? "",
            RefreshToken = userWithToken.Value.refreshToken ?? ""
        });
    }

    [HttpPost("refreshToken")]
    public async Task<IActionResult> RefreshToken(string refreshToken)
    {
        var response = await _authService.RefreshToken(refreshToken);

        if (response.HasValue) {
            return Ok(new AuthResponse {
                Username = response.Value.user?.Username ?? "",
                Token = response.Value.token ?? "",
                RefreshToken = response.Value.refreshToken ?? ""
            });
        }

        return BadRequest("Invalid or revoked token");
    }
}