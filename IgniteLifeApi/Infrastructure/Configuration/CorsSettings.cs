using System.ComponentModel.DataAnnotations;

namespace IgniteLifeApi.Infrastructure.Configuration
{
    public class CorsSettings
    {
        [Url(ErrorMessage = "SpaOrigin must be a valid URL.")]
        public string? SpaOrigin { get; set; }
    }
}
