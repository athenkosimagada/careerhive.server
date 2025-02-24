namespace careerhive.domain.Exceptions;
public class RefreshTokenValidationException : Exception
{
    public RefreshTokenValidationException(string message) : base(message) { }
}
