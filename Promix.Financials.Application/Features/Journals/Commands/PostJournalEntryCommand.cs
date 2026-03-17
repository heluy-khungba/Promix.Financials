namespace Promix.Financials.Application.Features.Journals.Commands;

public sealed record PostJournalEntryCommand(Guid CompanyId, Guid EntryId);
