using System.Net;
using System.Text;
using Scriban;

namespace Server.Services.ServerServices;

public class TemplateView : IActionResult
{
    private readonly string _viewName;
    private readonly object _templateModel;
    private readonly IEnumerable<Cookie> _cookieToSet;

    public TemplateView(string viewName, object templateTemplateModel, params Cookie[] cookies)
    {
        if (string.IsNullOrEmpty(viewName))
            throw new ArgumentException("TemplateView name must be not nul or empty!");
        
        _viewName = viewName;
        _templateModel = templateTemplateModel;
        _cookieToSet = cookies;
    }

    public async Task ExecuteResultAsync(HttpListenerContext context)
    {
        if (!Directory.Exists("./Views"))
            throw new ArgumentException("To use TemplateView you must assign 'Views' folder in root directory!");

        if (!File.Exists($"./Views/{_viewName}.cshtml"))
            throw new FileNotFoundException($"Requested view '{_viewName}.cshtml' is not found!");
        
        var cshtml = await File.ReadAllTextAsync($"./Views/{_viewName}.cshtml");
        var template = Template.Parse(cshtml);
        cshtml = await template.RenderAsync(_templateModel);
        var response = context.Response;
        var writeToBody = response.WriteToBodyAsync(Encoding.UTF8.GetBytes(cshtml));
        response.SetContentType(".html");

        foreach (var cookie in _cookieToSet)
        {
            response.Cookies.Add(cookie);
        }
        
        await writeToBody;
    }
}