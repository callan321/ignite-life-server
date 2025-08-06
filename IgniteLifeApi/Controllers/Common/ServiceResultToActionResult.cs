using IgniteLifeApi.Application.Services.Common;
using Microsoft.AspNetCore.Mvc;

namespace IgniteLifeApi.Controllers.Common
{
    public static class ServiceResultToActionResult
    {
        public static IActionResult ToActionResult<T>(ControllerBase controller, ServiceResult<T> result)
        {
            var loggerFactory = controller.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("ServiceResultToActionResult");

            if (result.Status == ServiceResultStatus.InternalServerError)
            {
                logger.LogError("Internal server error: {Message}", result.Message);
            }

            return result.Status switch
            {
                ServiceResultStatus.Success => controller.Ok(result.Data),
                ServiceResultStatus.Created => controller.Created(string.Empty, result.Data),
                ServiceResultStatus.NoContent => controller.NoContent(),
                ServiceResultStatus.BadRequest => controller.BadRequest(new { error = result.Message }),
                ServiceResultStatus.NotFound => controller.NotFound(new { error = result.Message }),
                ServiceResultStatus.Conflict => controller.Conflict(new { error = result.Message }),
                ServiceResultStatus.InternalServerError =>
                    controller.StatusCode(500, new { error = result.Message }),
                _ => controller.StatusCode(result.StatusCode, new { error = result.Message })
            };
        }
    }
}
