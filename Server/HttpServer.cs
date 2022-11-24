using Server.Services;
using Server.Services.ServerServices;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using Server.Services.ServerServices.CustomExceptions;

namespace Server
{
    public class HttpServer
    {
        public readonly string path;
        public readonly int _port;
        readonly ILogger logger;
        readonly HttpListener listener = new HttpListener();
        
        private readonly SessionManager _sessionManager = SessionManager.Instance;

        private static IEnumerable<ControllerInfo> _controllers = Assembly.GetExecutingAssembly().GetTypes()
            .Select(type => (Class: type, Attribute: type.GetCustomAttribute<ApiControllerAttribute>()))
            .Where(tuple => tuple.Attribute != null)
            .Select(x => new ControllerInfo(x.Class, x.Attribute!))
            .ToArray();


        public HttpServer(ILogger logger, ServerSettings settings)
        {
            this.logger = logger;
            listener.Prefixes.Add($"http://localhost:{settings.Port}/");
            path = settings.Path;
            _port = settings.Port;
        }

        public async Task Start()
        {
            logger.Log("Starting server...");
            listener.Start();
            logger.Log($"Server started at port {_port}");
            while (true)
            {
                try
                {
                    var context = await listener.GetContextAsync();
                    HandleRequestAsync(context);
                }
                catch(Exception ex)
                {
                    logger.ReportError($"Unhandled exception: {ex.Message}");
                }
            }
        }
        
        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var fileSent = false;
            
            if(request.HttpMethod == "GET")
                fileSent = await SendFileIfExistsAsync(context);
            
            if(fileSent)  return;

            try
            {
                await GiveContextToControllerByRouteIfExists(context);
            }
            catch
            {
                logger.Log($"Can not handle request {context.Request.Url}");
                context.Response
                    .Write404PageToBody()
                    .SetStatusCode((int)HttpStatusCode.NotFound)
                    .SetContentType(".html")
                    .Close();
            }

            logger.Log($"\n{request.ProtocolVersion} {request.HttpMethod} {request.Url} {context.Response.StatusCode}\n");
        }

        private async Task GiveContextToControllerByRouteIfExists(HttpListenerContext context)
        {
            var request = context.Request;
            var httpMethod = request.HttpMethod.ToUpper();
            var segments = context!.Request!.Url!.Segments;

            if (segments.Length < 1) return;
            segments[segments.Length - 1] = RemoveSlashes(segments[segments.Length - 1])!;

            var tuple = GetControllerAndMethodRoute(segments);
            var controllerRoute = tuple.Item1;
            var methodRoute = tuple.Item2;

            var controller = GetRequiredController(controllerRoute);

            if (controller == null)
            {
                await ActionResultFactory.NotFound().ExecuteResultAsync(context);
                return;
            }
            
            if (string.IsNullOrEmpty(methodRoute))
                methodRoute = controller.Name.Replace("Controller", "");

            var markedMethods = controller.GetMethods()
                .Where(x => x.GetCustomAttribute(typeof(ApiControllerMethodAttribute)) != null);
            var parameters = GetParametersFromQuery(context);
            var method = GetRequiredMethod(httpMethod, markedMethods, methodRoute, parameters.Keys);

            if (method == null)
            {
                await ActionResultFactory.NotFound().ExecuteResultAsync(context);
                return;
            }

            Session? session = null;
            if (method.GetCustomAttribute<AuthorizeRequiredAttribute>() == null 
                || IsAuthorized(context.Request.Cookies, out session))
            {
                var methodParameters = method.GetParameters();
                var methodParametersAttributesTypes = methodParameters
                    .SelectMany(x => x.CustomAttributes.Select(x => x.AttributeType));

                if (methodParametersAttributesTypes.Any(x => x == typeof(CookieRequiredAttribute)))
                    AddValueToParameters(parameters, context.Request.Cookies, methodParameters, typeof(CookieRequiredAttribute));
                if (methodParametersAttributesTypes.Any(x => x== typeof(SessionRequiredAttribute)))
                    AddValueToParameters(parameters, session, methodParameters, typeof(SessionRequiredAttribute));
                var actionResult = GetActionResultTaskFromMethod(
                    controller.GetConstructor(new Type[0]).Invoke(Array.Empty<object>()), method, parameters);
                await actionResult.ExecuteResultAsync(context);
            }
            else await ActionResultFactory.Unauthorized().ExecuteResultAsync(context);
        }
        
        private void AddValueToParameters<T>(Dictionary<string, object> parameters, 
            T value, ParameterInfo[] methodParameters, Type attributeType)
        {
            foreach (var parameter in methodParameters)
            {
                if(parameter.GetCustomAttribute(attributeType) != null)
                {
                    parameters.Add(parameter.Name!, value!);
                    break;
                }
            }
        }
        
        private bool IsAuthorized(CookieCollection cookies, out Session? session)
        {
            session = null;
            if (cookies["SessionId"] != null)
                return Guid.TryParse(cookies["SessionId"]?.Value, out var guid) 
                       && SessionManager.Instance.TryGetSession(guid, out session);

            return false;
        }


        private IActionResult GetActionResultTaskFromMethod(object controller,
            MethodInfo method, Dictionary<string, object> parameters)
        {

            var paramsIn = method.GetParameters()
                .Select(p => Convert.ChangeType(parameters[p.Name], p.ParameterType))
                .ToArray();

            return (IActionResult)method.Invoke(controller, paramsIn);
        }
        
        private Dictionary<string, object> GetParametersFromQuery(HttpListenerContext context)
        {
            IEnumerable<(string Key, object Value)> keyValues;
            var method = context.Request.HttpMethod;
            if (method == "GET")
                keyValues = context.Request.QueryString.AllKeys
                    .Where(x => !string.IsNullOrEmpty(x) && !string.IsNullOrEmpty(context.Request.QueryString[x]))
                    .Select(x => (Key: x!, Value: context.Request.QueryString[x]! as object));
            else if (method == "POST")
            {
                using var body = context.Request.InputStream;
                using var reader = new StreamReader(body, context.Request.ContentEncoding);
                var parameters = HttpUtility.UrlDecode(reader.ReadToEnd(), Encoding.UTF8);
                keyValues = parameters
                    .Split("&", StringSplitOptions.RemoveEmptyEntries)
                    .Select(x =>
                    {
                        var keyAndValue = x.Split('=', StringSplitOptions.RemoveEmptyEntries);
                        return (Key: keyAndValue.FirstOrDefault()!, Value: keyAndValue.LastOrDefault()!);
                    })
                    .Where(x => !string.IsNullOrEmpty(x.Key) && !string.IsNullOrEmpty(x.Value))
                    .Select(x=> (Key: x.Key, Value: x.Value as object));
            }
            else throw new UnsupportedHttpMethodException($"{method} http method was requested.");

            return keyValues.ToDictionary(x=> x.Key, x=> x.Value);
        }


        private Type? GetRequiredController(string controllerRoute)
        {
            return _controllers.Where(x=> string.IsNullOrEmpty(x.Attribute.ControllerRoute)
                                            ? x.Class.Name.Replace("Controller", "") == controllerRoute
                                            : x.Attribute.ControllerRoute == controllerRoute)
                                     .Select(x => x.Class)
                                     .FirstOrDefault();
        }

        private MethodInfo? GetRequiredMethod
            (string httpMethod, IEnumerable<MethodInfo?> markedMethods, string route, IEnumerable<string> parameters)
        {
            return httpMethod switch
            {
                "GET" => Selector(typeof(HttpGetAttribute), markedMethods, route, parameters),
                "POST" => Selector(typeof(HttpPostAttribute), markedMethods, route, parameters),
                _ => null,
            };
        }

        private MethodInfo? Selector(Type type, IEnumerable<MethodInfo?> markedMethods, string route, IEnumerable<string> parameters)
        {
            return markedMethods
                .Select(
                    x => (x, x?.GetCustomAttribute(type) as ApiControllerMethodAttribute))
                .Where(x => x.Item2 != null 
                            && (string.IsNullOrEmpty(x.Item2.MethodURI) 
                                ? x.Item1!.Name == route : x.Item2.MethodURI == route))
                .Select(x => x.Item1)
                .FirstOrDefault(x => x.GetParameters()
                    .Where(x => x.GetCustomAttribute<FromQueryAttribute>() != null)
                    .All(param => parameters.Contains(param.Name)));
        }
        
        private (string?, string?) GetControllerAndMethodRoute(string[] segments)
        {
            string? method = null;
            string? controller;

            if (segments.Length == 1 && segments[0] == String.Empty)
                controller = "/";
            else if (segments.Length == 3)
            {
                controller = RemoveSlashes(segments[1]);
                method = segments[2];
            }
            else if(segments.Length == 2 && segments[0] == "/" && !string.IsNullOrEmpty(segments[1]))
            {
                controller = "/";
                method = string.Concat(segments[1].TakeWhile(x => x != '?'));
            }
            else
            {
                controller = RemoveSlashes(segments[1]);
                method = string.Concat(segments.Skip(2).TakeWhile(x => x != "?"));
            }

            return (controller, method);
        }

        private async Task<bool> SendFileIfExistsAsync(HttpListenerContext context)
        {
            var response = context.Response;
            if(!Directory.Exists(path))
                return false;

            var rawUrl = HttpUtility.UrlDecode(context.Request.RawUrl, Encoding.UTF8);
            var bufferExtensionTuple = FileProvider.GetFileAndFileExtension(path + rawUrl);
            var buffer = bufferExtensionTuple.Item1;
            var extension = bufferExtensionTuple.Item2;
                
            if (buffer is null)
                return false;
            
            await HttpListenerResponseExtensions.WriteToBodyAsync(response.OutputStream, buffer);
            response.SetContentType(extension)
                .SetStatusCode(200)
                .Close();
            return true;
        }
        
        private static string? RemoveSlashes(string? input) => input?.Replace("/", string.Empty);
    }
}