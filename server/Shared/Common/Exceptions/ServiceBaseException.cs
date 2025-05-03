using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Common.Exceptions;

/// <summary>
/// Base exception class for all service-related exceptions in the application.
/// </summary>
/// <remarks>
/// This exception serves as the base for more specific custom exceptions.
/// It provides common properties like ErrorCode and StatusCode that can be
/// used to provide more detailed error information to clients.
/// </remarks>
public class ServiceBaseException : Exception
{
    /// <summary>
    /// Gets the error code associated with this exception.
    /// </summary>
    /// <value>
    /// A string representing a specific error code or null if no code is specified.
    /// This can be used by clients to identify specific error conditions.
    /// </value>
    public string? ErrorCode { get; }

    /// <summary>
    /// Gets the HTTP status code that should be returned to the client.
    /// </summary>
    /// <value>
    /// An integer representing an HTTP status code. If 0, the middleware
    /// will determine an appropriate status code based on the exception type.
    /// </value>
    public int StatusCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceBaseException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="errorCode">A specific error code to help identify the error condition (optional).</param>
    /// <param name="statusCode">The HTTP status code to return to the client (optional).</param>
    /// <example>
    /// <code>
    /// throw new ServiceBaseException("User not found", "USER_NOT_FOUND", 404);
    /// </code>
    /// </example>
    public ServiceBaseException(string message, string? errorCode = null, int statusCode = 0)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}
