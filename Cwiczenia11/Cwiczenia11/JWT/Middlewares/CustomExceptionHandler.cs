using System.Net.Http.Headers;
using System.Text;
namespace JWT.Middlewares;

public class CustomExceptionHandler
{
    private readonly RequestDelegate _next;
    public CustomExceptionHandler(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                var authorizationHeader = AuthenticationHeaderValue.Parse(context.Request.Headers["Authorization"]!);

                if (authorizationHeader.Scheme.Equals("basic", StringComparison.OrdinalIgnoreCase) && authorizationHeader.Parameter != null)
                {
                    var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authorizationHeader.Parameter)).Split(':');
                    var userName = credentials[0];
                    var userPassword = credentials[1];

                    if (IsUserAuthorized(userName, userPassword))
                    {
                        await _next(context);
                        return;
                    }
                }
            }

            context.Response.Headers["WWW-Authenticate"] = $"Basic realm=\"Authentication\"";
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
        catch (Exception ex)
        {
            // Log the exception (implement logging as needed)
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("An unexpected error occurred. Please try again later.");
        }
    }

    private bool IsUserAuthorized(string userName, string userPassword)
    {
        return userName == "admin" && userPassword == "qwerty";
    }
}