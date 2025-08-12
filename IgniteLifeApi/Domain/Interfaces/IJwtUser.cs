namespace IgniteLifeApi.Domain.Interfaces
{
    public interface IJwtUser
    {
        Guid Id { get; }
        string Email { get; }
    }
}
