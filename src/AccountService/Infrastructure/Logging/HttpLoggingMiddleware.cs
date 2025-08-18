namespace AccountService.Infrastructure.Logging;

public sealed class HttpLoggingMiddleware(RequestDelegate next, ILogger<HttpLoggingMiddleware> log)
{
    public async Task Invoke(HttpContext ctx)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await next(ctx);
        sw.Stop();
        log.LogInformation("http {Method} {Path} {Status} corr={Corr} latency_ms={Ms}",
            ctx.Request.Method, ctx.Request.Path, ctx.Response.StatusCode,
            ctx.Items[CorrelationIdMiddleware.HeaderName], sw.ElapsedMilliseconds);
    }
}