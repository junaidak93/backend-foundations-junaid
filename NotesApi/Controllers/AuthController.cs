using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtTokenGenerator _jwt;

    public AuthController(IAuthService authService, IJwtTokenGenerator jwt)
    {
        _authService = authService;
        _jwt = jwt;
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
        var user = await _authService.Login(req.Username, req.Password);
        if (user == null) return Unauthorized();

        var token = _jwt.GenerateToken(user);
        return Ok(new AuthResponse { Username = user.Username, Token = token });
    }
}