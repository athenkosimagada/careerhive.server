namespace careerhive.domain.Exceptions;
public class TwoFactorRequiredException : Exception
{
    public TwoFactorRequiredException(string message) : base(message) { }
}
