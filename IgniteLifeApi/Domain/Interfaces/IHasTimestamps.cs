namespace IgniteLifeApi.Domain.Interfaces
{
    public interface IHasTimestamps
    {
        DateTime CreatedAtUtc { get; set; }
        DateTime UpdatedAtUtc { get; set; }
    }
}
