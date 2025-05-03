using System.Net;
using Shared.Common.Exceptions;

namespace UserManagement.Models.Exceptions;

/// <summary>
/// Exception specific to authentication and authorization errors.
/// </summary>
/// <remarks>
/// This exception should be thrown when there are issues with user authentication,
/// such as invalid credentials, expired tokens, or insufficient permissions.
/// By default, it sets the HTTP status code to 401 Unauthorized.
/// </remarks>
public class AuthException : ServiceBaseException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthException"/> class
    /// with default message and error code.
    /// </summary>
    /// <remarks>
    /// Use this constructor when you don't need to specify a custom message.
    /// It will set a generic error message and the AUTH_EXCEPTION error code.
    /// </remarks>
    /// <example>
    /// <code>
    /// throw new AuthException();
    /// </code>
    /// </example>
    public AuthException() : base("An error occurred while authenticating the user", "AUTH_EXCEPTION", (int)HttpStatusCode.Unauthorized) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthException"/> class
    /// with a specified error message and default error code.
    /// </summary>
    /// <param name="message">The custom error message explaining the authentication issue.</param>
    /// <remarks>
    /// Use this constructor when you need to provide a specific error message
    /// but want to use the default AUTH_EXCEPTION error code.
    /// </remarks>
    /// <example>
    /// <code>
    /// throw new AuthException("Invalid username or password");
    /// </code>
    /// </example>
    public AuthException(string message) : base(message, "AUTH_EXCEPTION", (int)HttpStatusCode.Unauthorized) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthException"/> class
    /// with a specified error message and error code.
    /// </summary>
    /// <param name="message">The custom error message explaining the authentication issue.</param>
    /// <param name="errorCode">A specific error code to identify the type of authentication error.</param>
    /// <remarks>
    /// Use this constructor when you need to customize both the error message and error code,
    /// while keeping the default 401 Unauthorized status code.
    /// </remarks>
    /// <example>
    /// <code>
    /// throw new AuthException("Token has expired", "TOKEN_EXPIRED");
    /// </code>
    /// </example>
    public AuthException(string message, string errorCode) : base(message, errorCode, (int)HttpStatusCode.Unauthorized) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthException"/> class
    /// with a specified error message, error code, and HTTP status code.
    /// </summary>
    /// <param name="message">The custom error message explaining the authentication issue.</param>
    /// <param name="errorCode">A specific error code to identify the type of authentication error.</param>
    /// <param name="statusCode">The HTTP status code to return (e.g., 401, 403).</param>
    /// <remarks>
    /// Use this constructor for complete customization of the exception.
    /// This is useful for different authentication scenarios that might require
    /// different status codes (e.g., 403 Forbidden for permission issues).
    /// </remarks>
    /// <example>
    /// <code>
    /// throw new AuthException("User does not have required permissions", "INSUFFICIENT_PERMISSIONS", (int)HttpStatusCode.Forbidden);
    /// </code>
    /// </example>
    public AuthException(string message, string errorCode, int statusCode) : base(message, errorCode, statusCode) { }
}