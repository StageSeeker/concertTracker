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
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            var auth = context.User?.Identity?.IsAuthenticated ?? false;
            if (!auth)
            {
                context.Response.Redirect("/login");
                return;
            }
        }
        await next(context);
    }
}