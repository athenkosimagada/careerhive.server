namespace careerhive.domain.Exceptions;
public class LockedOutException : Exception
{
    public LockedOutException(string message) : base(message) { }
}
