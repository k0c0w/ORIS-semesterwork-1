using System.Net;
using System.Text;
using System.Text.Json;

namespace Server.Services.ServerServices;

public static class ActionResultFactory
{
    public static IActionResult SendHtml(string html) => new HtmlResult(html); 
    public static IActionResult SendHtml(string html, SessionInfo sessionInfo) => new HtmlResult(html, sessionInfo);
    public static IActionResult SendHtml(byte[] html) => new HtmlResult(html);

    public static IActionResult RedirectTo(string redirectTo) => new Redirect(redirectTo);

    public static IActionResult Json<T>(T model) => new Json<T>(model);

    public static IActionResult NotFound() => new NotFound();

    public static IActionResult Unauthorized() => new Unauthorized();
}

public class NotFound : IActionResult
{
    public Task ExecuteResultAsync(HttpListenerContext context)
    {
        context.Response.SetStatusCode((int)HttpStatusCode.NotFound)
            .Write404PageToBody()
            .Close();
        return Task.CompletedTask;
    }
}

public class Unauthorized : IActionResult
{
    public Task ExecuteResultAsync(HttpListenerContext context)
    {
        context.Response.SetStatusCode((int)HttpStatusCode.Unauthorized);
        return Task.CompletedTask;
    }
}

public class HtmlResult : IActionResult
{
    private readonly byte[] _htmlBytes;
    private readonly SessionInfo? _sessionInfo;

    public HtmlResult(string html, SessionInfo? sessionInfo = null) : this(Encoding.UTF8.GetBytes(html), sessionInfo) {}

    public HtmlResult(byte[] file, SessionInfo? sessionInfo = null)
    {
        _htmlBytes = file;
        _sessionInfo = sessionInfo;
    }
    
    public Task ExecuteResultAsync(HttpListenerContext context)
    {
        return Task.Run(async () =>
        {
            var response = context.Response.SetStatusCode((int)HttpStatusCode.OK)
                .SetContentType(".html");
            if (_sessionInfo != null)
            {
                response.Cookies.Add(new Cookie("SessionId", $"{_sessionInfo.Guid}"));
            }

            await response.WriteToBodyAsync(_htmlBytes);
            response.Close();
        });
    }
}

public class Redirect : IActionResult
{
    private readonly string _route;
    public Redirect(string route) => _route = route;
    
    public Task ExecuteResultAsync(HttpListenerContext context)
    {
        var response = context.Response.SetStatusCode((int)HttpStatusCode.SeeOther);
        response.Headers.Set("Location", _route);
        response.Close(); 
        return Task.CompletedTask;
    }
}

public class Json<T> : IActionResult
{
    private readonly T _model;

    public Json(T model) => _model = model;

    public Task ExecuteResultAsync(HttpListenerContext context)
    {
        return Task.Run(async () =>
            {
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, _model);
                context.Response.SetStatusCode((int)HttpStatusCode.OK)
                    .SetContentType(".json")
                    .Close();
            });
    }
}