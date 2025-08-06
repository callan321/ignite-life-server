namespace IgniteLifeApi.Application.Services.Common
{
    public enum ServiceResultStatus
    {
        Success,            // 200 OK or similar
        Created,            // 201 Created
        NoContent,          // 204 No Content
        BadRequest,         // 400
        Unauthorized,       // 401
        Forbidden,          // 403
        NotFound,           // 404
        Conflict,           // 409
        UnsupportedMediaType, // 415
        InternalServerError, // 500
        ServiceUnavailable  // 503
    }

    public class ServiceResult<T>
    {
        public ServiceResultStatus Status { get; set; }
        public int StatusCode => GetStatusCode(Status);
        public T? Data { get; set; }
        public string? Message { get; set; }

        private static int GetStatusCode(ServiceResultStatus status) => status switch
        {
            ServiceResultStatus.Success => 200,
            ServiceResultStatus.Created => 201,
            ServiceResultStatus.NoContent => 204,
            ServiceResultStatus.BadRequest => 400,
            ServiceResultStatus.Unauthorized => 401,
            ServiceResultStatus.Forbidden => 403,
            ServiceResultStatus.NotFound => 404,
            ServiceResultStatus.Conflict => 409,
            ServiceResultStatus.UnsupportedMediaType => 415,
            ServiceResultStatus.InternalServerError => 500,
            ServiceResultStatus.ServiceUnavailable => 503,
            _ => 500
        };

        // Factory methods for easy creation:
        public static ServiceResult<T> SuccessResult(T data, string? message = null) =>
            new ServiceResult<T> { Status = ServiceResultStatus.Success, Data = data, Message = message };

        public static ServiceResult<T> CreatedResult(T data, string? message = null) =>
            new ServiceResult<T> { Status = ServiceResultStatus.Created, Data = data, Message = message };

        public static ServiceResult<T> NoContentResult(string? message = null) =>
            new ServiceResult<T> { Status = ServiceResultStatus.NoContent, Data = default, Message = message };

        public static ServiceResult<T> BadRequest(string message) =>
            new ServiceResult<T> { Status = ServiceResultStatus.BadRequest, Message = message };

        public static ServiceResult<T> Unauthorized(string message) =>
            new ServiceResult<T> { Status = ServiceResultStatus.Unauthorized, Message = message };

        public static ServiceResult<T> Forbidden(string message) =>
            new ServiceResult<T> { Status = ServiceResultStatus.Forbidden, Message = message };

        public static ServiceResult<T> NotFound(string message) =>
            new ServiceResult<T> { Status = ServiceResultStatus.NotFound, Message = message };

        public static ServiceResult<T> Conflict(string message) =>
            new ServiceResult<T> { Status = ServiceResultStatus.Conflict, Message = message };

        public static ServiceResult<T> UnsupportedMediaType(string message) =>
            new ServiceResult<T> { Status = ServiceResultStatus.UnsupportedMediaType, Message = message };

        public static ServiceResult<T> InternalServerError(string message) =>
            new ServiceResult<T> { Status = ServiceResultStatus.InternalServerError, Message = message };

        public static ServiceResult<T> ServiceUnavailable(string message) =>
            new ServiceResult<T> { Status = ServiceResultStatus.ServiceUnavailable, Message = message };
    }
}
