namespace careerhive.domain.Exceptions;
internal class UnauthorizedAccessException : Exception
{
    public UnauthorizedAccessException(string message) : base(message) { }
}
