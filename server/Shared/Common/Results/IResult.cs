using System.Net;
using Shared.Common.Models.Pagination;
using Shared.Common.Models.Performance;

namespace Shared.Common.Results
{
    public interface IResult
    {
        bool IsSuccess { get; }
        bool IsFailure { get; }
        List<string> Messages { get; }
        string Error { get; }
        HttpStatusCode StatusCode { get; }

        // Exception handling
        Exception Exception { get; }
        bool HasException { get; }

        // Caching information
        string ETag { get; }
        DateTime? LastModified { get; }
        TimeSpan? CacheDuration { get; }

        // Performance metrics
        PerformanceMetrics PerformanceMetrics { get; }
    }

    public interface IResult<out T> : IResult
    {
        T Data { get; }
        PaginationMetadata PaginationMetadata { get; }
    }
}