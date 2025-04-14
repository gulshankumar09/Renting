using System.Net;
using System.Security.Cryptography;
using System.Text;
using Shared.Common.Models.Pagination;
using Shared.Common.Models.Performance;

namespace Shared.Common.Results
{
    public class Result : IResult
    {
        public bool IsSuccess { get; protected set; }
        public bool IsFailure => !IsSuccess;
        public List<string> Messages { get; protected set; } = new List<string>();
        public string Error { get; protected set; }
        public HttpStatusCode StatusCode { get; protected set; }
        public Dictionary<string, List<string>> ValidationErrors { get; protected set; }

        // Exception handling
        public Exception Exception { get; protected set; }
        public bool HasException => Exception != null;

        // Caching information
        public string ETag { get; protected set; }
        public DateTime? LastModified { get; protected set; }
        public TimeSpan? CacheDuration { get; protected set; }

        // Performance metrics
        public PerformanceMetrics PerformanceMetrics { get; protected set; }

        protected Result()
        {
            ValidationErrors = new Dictionary<string, List<string>>();
            PerformanceMetrics = new PerformanceMetrics();
        }

        protected Result(bool isSuccess, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            IsSuccess = isSuccess;
            StatusCode = statusCode;
            ValidationErrors = new Dictionary<string, List<string>>();
            PerformanceMetrics = new PerformanceMetrics();
        }

        protected Result(bool isSuccess, string message, HttpStatusCode statusCode = HttpStatusCode.OK)
            : this(isSuccess, statusCode)
        {
            if (isSuccess)
                Messages = [message];
            else
                Error = message;
        }

        protected Result(bool isSuccess, string message, Exception exception, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
            : this(isSuccess, message, statusCode)
        {
            Exception = exception;
        }

        protected Result(bool isSuccess, Dictionary<string, List<string>> validationErrors, HttpStatusCode statusCode = HttpStatusCode.UnprocessableEntity)
            : this(isSuccess, statusCode)
        {
            ValidationErrors = validationErrors;
            Error = "Validation failed";
        }

        public static Result Success()
        {
            return new Result(true, HttpStatusCode.OK);
        }

        public static Result Success(string message)
        {
            return new Result(true, message, HttpStatusCode.OK);
        }

        public static Result<T> Success<T>(T data)
        {
            return new Result<T>(true, data, HttpStatusCode.OK);
        }

        public static Result<T> Success<T>(T data, string message)
        {
            return new Result<T>(true, data, message, HttpStatusCode.OK);
        }

        public static Result Failure(string error, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            return new Result(false, error, statusCode);
        }

        public static Result<T> Failure<T>(string error, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            return new Result<T>(false, error, statusCode);
        }

        public static Result FromException(Exception exception)
        {
            var statusCode = exception switch
            {
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                ArgumentException => HttpStatusCode.BadRequest,
                KeyNotFoundException => HttpStatusCode.NotFound,
                _ => HttpStatusCode.InternalServerError
            };

            return new Result(false, exception.Message, exception, statusCode);
        }

        public static Result<T> FromException<T>(Exception exception)
        {
            var statusCode = exception switch
            {
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                ArgumentException => HttpStatusCode.BadRequest,
                KeyNotFoundException => HttpStatusCode.NotFound,
                _ => HttpStatusCode.InternalServerError
            };

            return new Result<T>(false, exception.Message, exception, statusCode);
        }

        public static Result ValidationFailure(Dictionary<string, List<string>> validationErrors)
        {
            return new Result(false, validationErrors, HttpStatusCode.UnprocessableEntity);
        }

        public Result AddMessage(string message)
        {
            Messages.Add(message);
            return this;
        }

        public Result WithStatusCode(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
            return this;
        }

        public Result WithCaching(TimeSpan duration)
        {
            CacheDuration = duration;
            LastModified = DateTime.UtcNow;

            // Generate ETag based on the current state
            var state = $"{IsSuccess}-{Error}-{string.Join(",", Messages)}-{LastModified}";
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(state));
                ETag = Convert.ToBase64String(hash);
            }

            return this;
        }

        public void CompleteMetrics()
        {
            PerformanceMetrics?.StopTracking();
        }

        public bool HasValidationErrors => ValidationErrors.Any();
    }

    public class Result<T> : Result, IResult<T>
    {
        public T Data { get; protected set; }
        public PaginationMetadata PaginationMetadata { get; protected set; }

        protected internal Result() : base() { }

        protected internal Result(bool isSuccess, T data, HttpStatusCode statusCode = HttpStatusCode.OK)
            : base(isSuccess, statusCode)
        {
            Data = data;
        }

        protected internal Result(bool isSuccess, T data, string message, HttpStatusCode statusCode = HttpStatusCode.OK)
            : base(isSuccess, message, statusCode)
        {
            Data = data;
        }

        protected internal Result(bool isSuccess, string error, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
            : base(isSuccess, error, statusCode)
        {
        }

        protected internal Result(bool isSuccess, string error, Exception exception, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
            : base(isSuccess, error, exception, statusCode)
        {
        }

        public new Result<T> AddMessage(string message)
        {
            Messages.Add(message);
            return this;
        }

        public new Result<T> WithStatusCode(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
            return this;
        }

        public new Result<T> WithCaching(TimeSpan duration)
        {
            base.WithCaching(duration);
            return this;
        }

        public Result<T> WithPagination(PaginationMetadata paginationMetadata)
        {
            PaginationMetadata = paginationMetadata;
            return this;
        }
    }
}