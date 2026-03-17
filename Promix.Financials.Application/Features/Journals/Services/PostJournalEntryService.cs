using Promix.Financials.Application.Abstractions;
using Promix.Financials.Application.Features.Journals.Commands;
using Promix.Financials.Domain.Exceptions;

namespace Promix.Financials.Application.Features.Journals.Services;

public sealed class PostJournalEntryService
{
    private readonly IJournalEntryRepository _entries;
    private readonly IUserContext _userContext;
    private readonly IDateTimeProvider _clock;

    public PostJournalEntryService(
        IJournalEntryRepository entries,
        IUserContext userContext,
        IDateTimeProvider clock)
    {
        _entries = entries;
        _userContext = userContext;
        _clock = clock;
    }

    public async Task PostAsync(PostJournalEntryCommand command, CancellationToken ct = default)
    {
        if (!_userContext.IsAuthenticated || _userContext.UserId == Guid.Empty)
            throw new BusinessRuleException("User is not authenticated.");

        var entry = await _entries.GetByIdAsync(command.CompanyId, command.EntryId, ct);
        if (entry is null)
            throw new BusinessRuleException("Journal entry not found.");

        entry.Post(_userContext.UserId, _clock.UtcNow);
        await _entries.SaveChangesAsync(ct);
    }
}
