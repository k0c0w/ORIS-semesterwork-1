using System.Net;
using Server.Models;
using Server.Services;
using Server.Services.ServerServices;

namespace Server.Controllers;

[ApiController("auth")]
public class AuthController
{
    private ORM _orm = new ORM();
    private SessionManager _sessionManager = SessionManager.Instance;
    
    [HttpPost("login")]
    public IActionResult Login([FromQuery] string email, [FromQuery] string password, [FromQuery] bool rememberMe,
        [CookieRequired] CookieCollection cookies)
    {
        if (!FormFieldValidator.IsEmailValid(email) || !FormFieldValidator.IsPasswordValid(password))
            return new Unauthorized();
        var sessionCookie = cookies["SessionId"]?.Value;
        var user = _orm.Select(new WhereModel<User>(new User() { Email = email, Password = password }))
            .FirstOrDefault();
        if (user != null)
        {
            var guid = Guid.Empty;
            var correctCookie = string.IsNullOrEmpty(sessionCookie) || Guid.TryParse(sessionCookie, out guid);
            if (correctCookie)
            {
                if (!_sessionManager.TryGetSession(guid, out var session) || session!.AccountId != user.Id)
                {
                    session = CreateNewSession((int)user.Id);
                    if (rememberMe)
                        _sessionManager.CreateLongSession(session.Id, () => session);
                    else
                        _sessionManager.CreateQuickSession(session.Id, () => session);
                }
                else if (rememberMe)
                {
                    _sessionManager.TerminateSession(session);
                    session = CreateNewSession((int)user.Id);
                    _sessionManager.CreateLongSession(session.Id, () => session );
                }
                return new Redirect("/", new SessionInfo {Guid = session.Id, LongLife = rememberMe});
            }
        }

        return new HtmlResult("Не авторизован");
    }


    [HttpPost("register")]
    public IActionResult Register([FromQuery] string email, [FromQuery] string password, [FromQuery] string firstName,
        [FromQuery] DateTime birthDate, [FromQuery] string accept)
    {
        var badFields = new List<InputError>(4);
        var dataProcessingAgreement = !string.IsNullOrEmpty(accept) && accept == "on";
        if(!FormFieldValidator.IsEmailValid(email))
            badFields.Add(new InputError(nameof(email), ErrorMessages.IncorrectEmailFormat));
        if(!FormFieldValidator.IsPasswordValid(password))
            badFields.Add(new InputError(nameof(password), ErrorMessages.PasswordShould));
        if (!FormFieldValidator.IsCorrectAge(birthDate))
            badFields.Add(new InputError(nameof(birthDate), ErrorMessages.IncorrectAge));
        if(!dataProcessingAgreement)
            badFields.Add(new InputError(nameof(accept), "Для регистрации необходимо Ваше согласие на обработку персональных данных!"));
        
        if(badFields.Any())
            return ReturnErrorJson(badFields);

        var emailExists = _orm.Select(new WhereModel<User>(new User { Email = email })).Any();
        if (emailExists)
        {
            badFields.Add(new InputError(nameof(email), ErrorMessages.EmailAlreadyUsed));
            return ReturnErrorJson(badFields);
        }

        var isCreated = TryCreateUser(email, firstName, password, birthDate, dataProcessingAgreement);
        if (isCreated)
            return new Redirect("/login");
        
        return new Json<OperationResultDto>(new OperationResultDto 
            { Errors = new []{ new InputError("server-error", "Ошибка регистрации.")}});
    }
    
    private IActionResult ReturnErrorJson(List<InputError> badFields) 
        => new Json<OperationResultDto>(new OperationResultDto {Errors = badFields.ToArray()});

    private bool TryCreateUser(string email, string firstName, string password, DateTime birtDate, bool agreement)
    {
        var user = new User
            { Email = email, Password = password, 
                BirthDate = birtDate, DataProcessingAgreement = agreement, FirstName = firstName};
        var insertedRows = _orm.Insert(user);
        return insertedRows != 0;
    }

    private Session CreateNewSession(int accountId) 
        => new Session { Id = Guid.NewGuid(), AccountId = accountId, CreateDateTime = DateTime.Now };
}