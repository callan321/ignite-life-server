namespace Server.Dtos;

public class ApiResult<T>
{
    public T? Data { get; set; }
    public string? Message { get; set; }
    public bool IsSuccess => Message == null;

    public static ApiResult<T> Success(T data) => new() { Data = data };
    public static ApiResult<T> Fail(string message) => new() { Message = message };
}

