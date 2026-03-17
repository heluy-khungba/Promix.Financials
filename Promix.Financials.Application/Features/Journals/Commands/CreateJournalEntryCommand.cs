using Promix.Financials.Domain.Enums;

namespace Promix.Financials.Application.Features.Journals.Commands;

public sealed record CreateJournalEntryLineCommand(
    Guid AccountId,
    decimal Debit,
    decimal Credit,
    string? Description
);

public sealed record CreateJournalEntryCommand(
    Guid CompanyId,
    DateOnly EntryDate,
    JournalEntryType Type,
    string? ReferenceNo,
    string? Description,
    bool PostNow,
    IReadOnlyList<CreateJournalEntryLineCommand> Lines
);
