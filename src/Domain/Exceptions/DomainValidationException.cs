namespace Domain.Exceptions;

/// <summary>
/// Thrown when input breaks a domain rule (e.g. title length). The API maps it to 400 Bad Request.
/// Use this instead of <see cref="ArgumentException"/>, which framework code also throws for server-side bugs.
/// </summary>
public class DomainValidationException : Exception
{
    public DomainValidationException(string message)
        : base(message)
    {
    }
}
