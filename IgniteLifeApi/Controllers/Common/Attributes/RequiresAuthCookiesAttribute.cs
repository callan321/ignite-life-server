namespace IgniteLifeApi.Controllers.Common.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class RequiresAuthCookiesAttribute : Attribute { }
