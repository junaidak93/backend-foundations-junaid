using Microsoft.AspNetCore.Mvc;
using NotesApi.Helpers;

[ApiController]
[Route("auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

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
        // After configuring ForwardedHeaders, this will correctly retrieve the client's IP
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var userAgent = Request.Headers.UserAgent.ToString();

        var userWithToken = await _authService.Login(req.Username, req.Password, ip, userAgent);
        
        if (!userWithToken.HasValue) 
            return Unauthorized();

        HttpContext.SetSecureHttpOnlyCookie
        (
            Constants.COOKIE_REFRESHTOKEN, 
            userWithToken.Value.refreshToken ?? "", 
            30 * 24 * 60
        );
        
        return Ok(new AuthResponse { 
            Username = userWithToken.Value.user?.Username ?? "", 
            Token = userWithToken.Value.token ?? "",
            RefreshToken = userWithToken.Value.refreshToken ?? ""
        });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(string refreshToken)
    {
        try 
        {
            // After configuring ForwardedHeaders, this will correctly retrieve the client's IP
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
            var userAgent = Request.Headers.UserAgent.ToString();

            var response = await _authService.RefreshToken(refreshToken, ip, userAgent);

            HttpContext.SetSecureHttpOnlyCookie
            (
                Constants.COOKIE_REFRESHTOKEN, 
                response.Value.refreshToken ?? "", 
                30 * 24 * 60
            );

            return Ok(new AuthResponse {
                Username = response.Value.user?.Username ?? "",
                Token = response.Value.token ?? "",
                RefreshToken = response.Value.refreshToken ?? ""
            });
        } 
        catch (Exception ex) 
        {
            return Unauthorized(new { error = ex.Message });
        }
    }
}