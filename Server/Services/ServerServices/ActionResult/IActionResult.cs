using System.Net;

namespace Server;

public interface IActionResult
{
    Task ExecuteResultAsync(HttpListenerContext context);
}