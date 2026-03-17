using Microsoft.EntityFrameworkCore;
using Promix.Financials.Application.Features.Journals.Queries;
using Promix.Financials.Domain.Aggregates.Accounts;
using Promix.Financials.Domain.Aggregates.Journals;

namespace Promix.Financials.Infrastructure.Persistence.Queries;

internal sealed class JournalEntriesQuery : IJournalEntriesQuery
{
    private readonly PromixDbContext _db;

    public JournalEntriesQuery(PromixDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<JournalEntrySummaryDto>> GetEntriesAsync(Guid companyId, CancellationToken ct = default)
    {
        return await _db.Set<JournalEntry>()
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.EntryDate)
            .ThenByDescending(x => x.CreatedAtUtc)
            .Select(x => new JournalEntrySummaryDto(
                x.Id,
                x.EntryNumber,
                x.EntryDate,
                (int)x.Type,
                (int)x.Status,
                x.ReferenceNo,
                x.Description,
                x.Lines.Sum(l => l.Debit),
                x.Lines.Sum(l => l.Credit),
                x.Lines.Count,
                x.CreatedAtUtc))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<JournalPostingAccountDto>> GetPostingAccountsAsync(Guid companyId, CancellationToken ct = default)
    {
        return await _db.Set<Account>()
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.IsPosting && x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new JournalPostingAccountDto(x.Id, x.Code, x.NameAr))
            .ToListAsync(ct);
    }
}
