namespace StageSeeker.MiddleWare;
public class SwaggerMiddleWare
{
    private readonly RequestDelegate next;

    public SwaggerMiddleWare(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var auth = context.User?.Identity?.IsAuthenticated ?? false;
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            if (!auth)
            {
                context.Response.Redirect("/login");
                return;
            }
        }
        if (context.Request.Path.StartsWithSegments("/profile") ||
        context.Request.Path.StartsWithSegments("/watchlist") ||
        context.Request.Path.StartsWithSegments("/users"))
        {
            if (!auth)
            {
                // Set status code to 401 for unauthorized
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                // Write the message to the response
                await context.Response.WriteAsync("Not authorized, please login");
                return;
            }
        }
        await next(context);
    }
}