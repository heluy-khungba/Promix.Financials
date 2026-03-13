namespace Promix.Financials.Application.Abstractions;

public sealed class AppSession
{
    public Guid UserId { get; set; }
    public Guid? CompanyId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }

    public bool IsExpired(DateTimeOffset nowUtc) => nowUtc >= ExpiresAtUtc;
}