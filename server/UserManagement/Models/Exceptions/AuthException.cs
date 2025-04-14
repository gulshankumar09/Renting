namespace UserManagement.Models.Exceptions;

public class AuthException : Exception
{
    public string? ErrorCode { get; }

    public AuthException() : base() { }

    public AuthException(string message) : base(message) { }

    public AuthException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    public AuthException(string message, Exception innerException) : base(message, innerException) { }
}