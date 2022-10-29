using System.Net;
using System.Text;

namespace Server.Services
{
    public static class HttpListenerResponseExtenssions
    {
        public static HttpListenerResponse SetContentType(this HttpListenerResponse response, string? extension = "")
        {
            response.Headers.Set("Content-Type", GetContentType(extension));
            return response;
        }

        public static HttpListenerResponse SetStatusCode(this HttpListenerResponse response, int statusCode)
        {
            response.StatusCode = statusCode;
            return response;
        }

        public static HttpListenerResponse Write403PageToBody(this HttpListenerResponse response)
        {
            var err = "403 - bad request.";
            WriteToBody(response.OutputStream, Encoding.UTF8.GetBytes(err));
            return response;
        }

        public static HttpListenerResponse Write404PageToBody(this HttpListenerResponse response)
        {
            var err = "404 - not found.";
            WriteToBody(response.OutputStream, Encoding.UTF8.GetBytes(err));
            return response;
        }

        public static HttpListenerResponse WriteToBody(this HttpListenerResponse response, byte[] buffer)
        {
            WriteToBody(response.OutputStream, buffer);
            return response;
        }

        public static async Task WriteToBodyAsync(Stream output, byte[] buffer)
        {
            using (output)
            {
                await output.WriteAsync(buffer, 0, buffer.Length);
                output.Close();
            }
        }

        public static void WriteToBody(Stream output, byte[] buffer)
        {
            using(output)
            {
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
        }

        private static string GetContentType(string extension)
        {
            switch (extension)
            {
                case ".htm":
                case ".html":
                    return "text/html";
                case ".css":
                    return "text/css";
                case ".js":
                    return "text/javascript";
                case ".jpg":
                    return "image/jpeg";
                case ".jpeg":
                case ".png":
                case ".gif":
                    return "image/" + extension.Substring(1);
                case ".ico":
                    return "image/x-icon";
                case ".svg":
                    return "image/svg+xml";
                case ".txt":
                    return "text/plain";

                default:
                    if (extension?.Length > 1)
                        return "application/" + extension.Substring(1);
                    else
                        return "application/unknown";
            }
        }
    }
}
