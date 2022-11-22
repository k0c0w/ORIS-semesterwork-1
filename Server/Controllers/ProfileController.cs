using System.Net;
using Server.Models;
using Server.MyORM;
using Server.Services.ServerServices;
namespace Server.Controllers;

[ApiController("profile")]
public class MyController
{
    [HttpGet("edit")]
    [AuthorizeRequired]
    public async Task<IActionResult> Edit([SessionRequired] Session userSession)
    {
        var orm = new ORM(@"Server=localhost;Database=ORIS-SW-1;Trusted_Connection=True;");
        var user = orm.Select(new WhereModel<User>(new User { Id = userSession.AccountId })).FirstOrDefault();
        var info = orm.Select(new WhereModel<PersonalInfo>(new PersonalInfo { UserId = userSession.AccountId })).FirstOrDefault();
        
        return new TemplateView("ProfileEdit", new { User = user, PersonalInfo = info });
    }

    [HttpGet("login")]
    public async Task<IActionResult> Login([CookieRequired] CookieCollection cookie)
    {
        var sessionCookie = cookie["SessionId"]?.Value;
        var _sessionManager = SessionManager.Instance;
        var accountExists = true;
        if (accountExists != null)
        {
            var guid = Guid.Empty;
            var correctCookie = string.IsNullOrEmpty(sessionCookie) || Guid.TryParse(sessionCookie, out guid);
            if (correctCookie && !_sessionManager.TryGetSession(guid, out var session))
            {
                guid = Guid.NewGuid();
                SessionManager.Instance.CreateSession(guid, 
              () => new Session{ Id = guid, AccountId = 2, CreateDateTime = DateTime.Now});
            }
                
            return ActionResultFactory.SendHtml("true", 
                new SessionInfo() { Guid = guid });
        }
            
        return ActionResultFactory.NotFound();
    }
}
