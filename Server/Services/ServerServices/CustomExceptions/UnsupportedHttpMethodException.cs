namespace Server.Services.ServerServices.CustomExceptions;

public class UnsupportedHttpMethodException : Exception
{
    public UnsupportedHttpMethodException(string message) : base(message) {}
    public UnsupportedHttpMethodException(string message, Exception inner) : base(message, inner) {}
}