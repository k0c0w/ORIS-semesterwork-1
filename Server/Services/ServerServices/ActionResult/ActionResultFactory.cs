using System.Net;
using System.Text;
using System.Text.Json;

namespace Server.Services.ServerServices;


public class Created : IActionResult
{
    public Task ExecuteResultAsync(HttpListenerContext context)
    {
        context.Response.SetStatusCode((int)HttpStatusCode.Created).Close();
        return Task.CompletedTask;
    }
}

public class BadRequest : IActionResult
{
    public Task ExecuteResultAsync(HttpListenerContext context)
    {
        context.Response.SetStatusCode((int)HttpStatusCode.BadRequest).Close();
        return Task.CompletedTask;
    }
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
                var cookie = new Cookie { Name = "SessionId", Value = $"{_sessionInfo.Guid}", Path = "/" };
                if(_sessionInfo.LongLife)
                    cookie.Expires = DateTime.Now.Add( TimeSpan.FromDays(180));
                
                response.Cookies.Add(cookie);
            }

            await response.WriteToBodyAsync(_htmlBytes);
            response.Close();
        });
    }
}

public class Redirect : IActionResult
{
    private readonly string _route;
    private readonly SessionInfo? _sessionInfo;
    public Redirect(string route, SessionInfo? sessionInfo=null)
    {
        _route = route;
        _sessionInfo = sessionInfo;
    }


    public Task ExecuteResultAsync(HttpListenerContext context)
    {
        var response = context.Response.SetStatusCode((int)HttpStatusCode.SeeOther);
        if (_sessionInfo != null)
        {
            var cookie = new Cookie { Name = "SessionId", Value = $"{_sessionInfo.Guid}", Path = "/" };
            if(_sessionInfo.LongLife)
                cookie.Expires = DateTime.Now.Add( TimeSpan.FromDays(180));
                
            response.Cookies.Add(cookie);
        }
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