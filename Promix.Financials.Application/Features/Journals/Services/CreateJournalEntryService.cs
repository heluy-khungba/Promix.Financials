using Promix.Financials.Application.Abstractions;
using Promix.Financials.Application.Features.Journals.Commands;
using Promix.Financials.Domain.Aggregates.Journals;
using Promix.Financials.Domain.Exceptions;

namespace Promix.Financials.Application.Features.Journals.Services;

public sealed class CreateJournalEntryService
{
    private readonly IJournalEntryRepository _entries;
    private readonly IAccountRepository _accounts;
    private readonly IUserContext _userContext;
    private readonly IDateTimeProvider _clock;

    public CreateJournalEntryService(
        IJournalEntryRepository entries,
        IAccountRepository accounts,
        IUserContext userContext,
        IDateTimeProvider clock)
    {
        _entries = entries;
        _accounts = accounts;
        _userContext = userContext;
        _clock = clock;
    }

    public async Task<Guid> CreateAsync(CreateJournalEntryCommand command, CancellationToken ct = default)
    {
        if (!_userContext.IsAuthenticated || _userContext.UserId == Guid.Empty)
            throw new BusinessRuleException("User is not authenticated.");

        if (command.CompanyId == Guid.Empty)
            throw new BusinessRuleException("CompanyId is required.");

        if (command.Lines is null || command.Lines.Count < 2)
            throw new BusinessRuleException("The journal entry must contain at least two lines.");

        var entryNumber = await _entries.GenerateNextNumberAsync(command.CompanyId, command.Type, ct);

        var entry = new JournalEntry(
            companyId: command.CompanyId,
            entryNumber: entryNumber,
            entryDate: command.EntryDate,
            type: command.Type,
            createdByUserId: _userContext.UserId,
            createdAtUtc: _clock.UtcNow,
            referenceNo: command.ReferenceNo,
            description: command.Description);

        foreach (var line in command.Lines)
        {
            var account = await _accounts.GetByIdAsync(line.AccountId, command.CompanyId);
            if (account is null)
                throw new BusinessRuleException("One of the selected accounts was not found.");

            if (!account.IsPosting)
                throw new BusinessRuleException($"Account {account.Code} must be a posting account.");

            if (!account.IsActive)
                throw new BusinessRuleException($"Account {account.Code} is inactive.");

            entry.AddLine(line.AccountId, line.Description, line.Debit, line.Credit);
        }

        if (command.PostNow)
            entry.Post(_userContext.UserId, _clock.UtcNow);

        await _entries.AddAsync(entry, ct);
        await _entries.SaveChangesAsync(ct);

        return entry.Id;
    }
}
