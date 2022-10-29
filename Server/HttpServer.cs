using System.Net;
using System.Reflection;
using Server.Services;

namespace Server
{
    public class HttpServer
    {
        public readonly string path;
        public readonly int _port;
        readonly ILogger logger;
        readonly HttpListener listener = new HttpListener();

        public static string Path { get; private set; }
        public bool IsRunning { get; private set; }

        public HttpServer(ILogger logger, ServerSettings settings)
        {
            this.logger = logger;
            listener.Prefixes.Add($"http://localhost:{settings.Port}/");
            path = settings.Path;
            Path = path;
            _port = settings.Port;
        }

        public async void Start()
        {
            try
            {
                if (IsRunning)
                {
                    logger.Log("Server is already  running!");
                    return;
                }

                logger.Log("Starting server...");
                listener.Start();
                IsRunning = true;
                logger.Log($"Server has started at port {_port}");

                while (true)
                    await HandleRequestAsync();
            }
            catch(Exception ex)
            {
                if (IsRunning)
                {
                    logger.ReportError($"Closing server since exception: {ex.Message}");
                }
            }
        }

        public void Stop()
        {
            listener.Stop();
            IsRunning = false;
            logger.Log($"Closed server at port {_port}...");
        }

        private async Task HandleRequestAsync()
        {
            var context = await listener.GetContextAsync();
            var request = context.Request;
            HttpListenerResponse response = null;

            if(request.HttpMethod.ToUpper() == "GET")
                response = await WriteFileIfExists(context);

            if (response == null)
                response = await GiveContextToControllerByRouteIfExists(context);

            if (response == null)
            {
                logger.Log($"Can not handle request {context.Request.Url}");
                response = context.Response
                                  .Write404PageToBody()
                                  .SetStatusCode((int)HttpStatusCode.NotFound)
                                  .SetContentType(".html");
            }

            logger.Log($"\n{request.ProtocolVersion} {request.HttpMethod} {request.Url} {response.StatusCode}\n");
            response.Close();
        }

        private async Task<HttpListenerResponse?> GiveContextToControllerByRouteIfExists(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            var httpMethod = request.HttpMethod.ToUpper();
            var segments = context!.Request!.Url!.Segments;

            if (segments.Length < 1) return null;
            segments[segments.Length - 1] = segments[segments.Length - 1].Replace("/","");

            var tuple = GetControllerAndMethodRoute(segments);
            string controllerRoute = tuple.Item1;
            string? methodRoute = tuple.Item2;

            var controllerType = typeof(ApiControllerAttribute);
            var controller = GetRequiredController(controllerType, controllerRoute, Assembly.GetExecutingAssembly());

            if (controller == null)
                return null;
            if (string.IsNullOrEmpty(methodRoute))
                methodRoute = controller.Name.Replace("Controller", "");

            var markedMethods = controller.GetMethods().Where(x => x.GetCustomAttribute(typeof(ApiControllerMethodAttribute)) != null);
            var method = GetRequiredMethod(httpMethod, markedMethods, methodRoute);

            if (method == null)
                return null;

            var controllerInstance = controller.GetConstructor(new Type[0]).Invoke(new object[0]);
            var response = method.Invoke(controllerInstance, new object[] { context }) as Task<HttpListenerResponse>;

            return await response;
        }

        private Type? GetRequiredController(Type controllerType, string controllerRoute, Assembly assembly)
        {
            return assembly.GetTypes()
                                     .Where(type => Attribute.IsDefined(type, controllerType))
                                     .Select(type => (type, type.GetCustomAttribute(controllerType) as ApiControllerAttribute))
                                     .Where(tuple =>
                                            tuple.Item2 != null
                                            && (string.IsNullOrEmpty(tuple.Item2.ControllerName)
                                            ? tuple.Item1.Name.Replace("Controller", "") == controllerRoute
                                            : tuple.Item2.ControllerName == controllerRoute))
                                     .Select(tuple => tuple.Item1)
                                     .FirstOrDefault();
        }

        private MethodInfo? GetRequiredMethod(string httpMethod, IEnumerable<MethodInfo?> markedMethods, string route)
        {
            Func<Type, MethodInfo?> selector = 
                (type) => markedMethods.Select(x => (x, x.GetCustomAttribute(type) as ApiControllerMethodAttribute))
                                       .Where(
                                            x => x.Item2 != null 
                                            && (string.IsNullOrEmpty(x.Item2.MethodURI) 
                                            ? x.Item1!.Name == route : x.Item2.MethodURI == route))
                                       .Select(x => x.Item1)
                                       .FirstOrDefault();
            switch (httpMethod)
            {
                case "GET":
                    return selector(typeof(HttpGetAttribute));
                case "POST":
                    return selector(typeof(HttpPostAttribute));
                default:
                    return null;
            }
        }

        private (string, string?) GetControllerAndMethodRoute(string[] segments)
        {
            Func<string, string> removeSlash = (word) => word.Replace("/", string.Empty); 
            string? method = null;
            string? controller = null;

            if (segments.Length == 1 && segments[0] == String.Empty)
                controller = "/";
            else if (segments.Length == 3)
            {
                controller = removeSlash(segments[1]);
                method = segments[2];
            }
            else
            {
                controller = removeSlash(segments[1]);
                method = string.Concat(segments.Skip(2).TakeWhile(x => x != "?"));
            }

            return (controller, method);
        }

        private async Task<HttpListenerResponse?> WriteFileIfExists(HttpListenerContext context)
        {
            var response = context.Response;
            if (Directory.Exists(path))
            {
                var rawUrl = context.Request.RawUrl.Replace("%20", " ");
                var bufferExtentionTuple = FileProvider.GetFileAndFileExtension(path + rawUrl);
                var buffer = bufferExtentionTuple.Item1;
                var extention = bufferExtentionTuple.Item2;
                if (buffer == null)
                {
                    return null;
                }
                await HttpListenerResponseExtenssions.WriteToBodyAsync(response.OutputStream, buffer);
                response.SetContentType(extention)
                        .SetStatusCode(200);
                return response;
            }
            return null;
        }
    }
}
