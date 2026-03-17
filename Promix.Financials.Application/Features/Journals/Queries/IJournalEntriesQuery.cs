namespace Promix.Financials.Application.Features.Journals.Queries;

public sealed record JournalEntrySummaryDto(
    Guid Id,
    string EntryNumber,
    DateOnly EntryDate,
    int Type,
    int Status,
    string? ReferenceNo,
    string? Description,
    decimal TotalDebit,
    decimal TotalCredit,
    int LineCount,
    DateTimeOffset CreatedAtUtc
);

public sealed record JournalPostingAccountDto(
    Guid Id,
    string Code,
    string NameAr
);

public interface IJournalEntriesQuery
{
    Task<IReadOnlyList<JournalEntrySummaryDto>> GetEntriesAsync(Guid companyId, CancellationToken ct = default);
    Task<IReadOnlyList<JournalPostingAccountDto>> GetPostingAccountsAsync(Guid companyId, CancellationToken ct = default);
}
