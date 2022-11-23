using System.Text.RegularExpressions;

namespace Server.Services;

public static class FormFieldValidator
{
    public static bool IsEmailValid(string? email)
    {
        if (string.IsNullOrEmpty(email) || email.Length > 254)
            return false;
        return Regex.IsMatch(email,
            @"^[-a-z0-9!#$%&'*+/=?^_`{|}~]+(\.[-a-z0-9!#$%&'*+/=?^_`{|}~]+)*@([a-z0-9]([-a-z0-9]{0,61}[a-z0-9])?\.)*(aero|arpa|asia|biz|cat|com|coop|edu|gov|info|int|jobs|mil|mobi|museum|name|net|org|pro|tel|travel|[a-z][a-z])$",
            RegexOptions.IgnoreCase, TimeSpan.FromSeconds(30d));
    }

    public static bool IsTelephoneNumberValid(string telephone)
    {
        if (telephone.Length != 9) return false;
        return Regex.IsMatch(telephone, "^[0-9]{9}$");
    }

    public static bool IsPassportValid(string passport)
    {
        return Regex.IsMatch(passport,
            @"^[0-9]{10}$",
            RegexOptions.IgnoreCase, TimeSpan.FromSeconds(30d));
    }
    
    public static bool IsDriverLicenseValid(string license)
    {
        return Regex.IsMatch(license,
            @"^[0-9]+$",
            RegexOptions.IgnoreCase, TimeSpan.FromSeconds(30d));
    }

    public static bool IsPasswordValid(string? password)
        => !string.IsNullOrEmpty(password) && 5 <= password.Length & password.Length <= 50
           && Regex.IsMatch(password, @"^([0-9]*[a-zA-Z]*)+$");

    public static bool IsCorrectAge(DateTime userBirthDate)
    {
        var now = DateTime.Today;
        var years = now.Year - userBirthDate.Year;
        var monthDelta = now.Month - userBirthDate.Month;
        var dayDelta = now.Day - userBirthDate.Day;
        var birthDayGone = monthDelta > 0 || monthDelta == 0 && dayDelta > 0;
        years = birthDayGone ? years : years - 1;

        return 18 <= years & years <= 90;
    }
}