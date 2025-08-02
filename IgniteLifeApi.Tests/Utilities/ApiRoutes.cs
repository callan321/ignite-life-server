namespace IgniteLifeApi.Tests.Utilities
{
    public static class ApiRoutes
    {
        /// <summary>
        /// Gets the base API route for a given controller type.
        /// It respects the default [Route("api/[controller]")] convention.
        /// </summary>
        /// <typeparam name="TController">The controller class.</typeparam>
        public static string ForController<TController>()
        {
            var name = typeof(TController).Name;
            return $"/api/{name.Replace("Controller", "")}";
        }

        /// <summary>
        /// Appends an action-specific segment to a controller's route.
        /// </summary>
        /// <typeparam name="TController">The controller class.</typeparam>
        /// <param name="actionSegment">The additional URL segment (e.g., "create", "123").</param>
        public static string ForController<TController>(string actionSegment)
        {
            var baseRoute = ForController<TController>();
            return $"{baseRoute}/{actionSegment}";
        }
    }
}
