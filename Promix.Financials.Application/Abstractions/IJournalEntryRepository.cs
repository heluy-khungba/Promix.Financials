using Promix.Financials.Domain.Aggregates.Journals;
using Promix.Financials.Domain.Enums;

namespace Promix.Financials.Application.Abstractions;

public interface IJournalEntryRepository
{
    Task AddAsync(JournalEntry entry, CancellationToken ct = default);
    Task<JournalEntry?> GetByIdAsync(Guid companyId, Guid entryId, CancellationToken ct = default);
    Task<string> GenerateNextNumberAsync(Guid companyId, JournalEntryType type, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
