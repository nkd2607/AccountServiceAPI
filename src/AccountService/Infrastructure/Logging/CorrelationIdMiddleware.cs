namespace AccountService.Infrastructure.Logging;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";
    public async Task Invoke(HttpContext ctx)
    {
        var cid = ctx.Request.Headers.TryGetValue(HeaderName, out var v) && !string.IsNullOrWhiteSpace(v)
            ? v.ToString()
            : Guid.NewGuid().ToString();
        ctx.Items[HeaderName] = cid;
        ctx.Response.Headers[HeaderName] = cid;
        await next(ctx);
    }
}