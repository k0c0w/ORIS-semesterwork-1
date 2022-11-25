using System.Net;
using System.Text;

namespace Server.Services.ServerServices.CustomExceptions;

public class ExceptionHandler
{
    public void Handle(MethodNotFoundException ex, HttpListenerResponse response)
    {
        response
            .Write404PageToBody()
            .SetStatusCode((int)HttpStatusCode.NotFound)
            .SetContentType(".html")
            .Close();
    }
    
    public void Handle(UnsupportedHttpMethodException ex, HttpListenerResponse response)
    {
        response.SetStatusCode((int)HttpStatusCode.MethodNotAllowed)
            .SetContentType(".html")
            .WriteToBody(Encoding.UTF8.GetBytes("405 - method not allowed"))
            .Close();
    }
    
    public void Handle(Exception ex, HttpListenerResponse response)
    {
        response.SetStatusCode((int)HttpStatusCode.InternalServerError)
            .SetContentType(".html")
            .WriteToBody(Encoding.UTF8.GetBytes("500 - server error occured"))
            .Close();
    }
}