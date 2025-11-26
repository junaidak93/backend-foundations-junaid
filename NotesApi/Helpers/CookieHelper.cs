namespace NotesApi.Helpers;

public static class CookieHelper
{
    public static void SetSecureHttpOnlyCookie(this HttpContext httpContext, string cookieName, string cookieValue, int expirationMinutes)
    {
        var cookieOptions = new CookieOptions
        {
            // Ensures the cookie is only sent over HTTPS.
            Secure = true, 

            // Prevents client-side scripts from accessing the cookie.
            HttpOnly = true, 

            // Sets the expiration time for the cookie.
            Expires = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes), 

            // Controls whether the cookie is sent with cross-site requests. 
            // Strict is recommended for most cases to prevent CSRF attacks.
            SameSite = SameSiteMode.Strict 
        };

        httpContext.Response.Cookies.Append(cookieName, cookieValue, cookieOptions);
    }
}