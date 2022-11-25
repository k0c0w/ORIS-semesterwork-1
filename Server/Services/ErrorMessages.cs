namespace Server.Services;

public static class ErrorMessages
{
    public const string IncorrectAge = "Вам должно быть не меньше 18 лет и не больше 90 лет.";
    public const string EmailAlreadyUsed = "Данная почта уже кем-то используется!";
    public const string IncorrectEmailFormat = "Неверный формат почты!";
    public const string IncorrectDateFormat = "Неверный формат даты!";
    public const string RequiredField = "Поле обязательно для заполнения!";
    public const string PasswordShould = "Пароль должен состоять из цифр и латинских символов (от 5 до 50).";
}