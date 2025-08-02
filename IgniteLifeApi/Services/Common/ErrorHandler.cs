namespace IgniteLifeApi.Services.Common
{
    public static class ErrorHandler
    {
        public static async Task<ServiceResult<T>> ExecuteWithErrorHandlingAsync<T>(
            Func<Task<ServiceResult<T>>> action,
            ILogger logger,
            string errorMessage)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ErrorMessage}", errorMessage);
                return ServiceResult<T>.InternalServerError(errorMessage);
            }
        }
    }
}
