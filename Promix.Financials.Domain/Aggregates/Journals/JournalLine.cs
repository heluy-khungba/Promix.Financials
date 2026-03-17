using Promix.Financials.Domain.Common;
using Promix.Financials.Domain.Aggregates.Accounts;
using Promix.Financials.Domain.Exceptions;

namespace Promix.Financials.Domain.Aggregates.Journals;

public sealed class JournalLine : Entity<Guid>
{
    private JournalLine() { }

    internal JournalLine(
        Guid journalEntryId,
        int lineNumber,
        Guid accountId,
        string? description,
        decimal debit,
        decimal credit)
    {
        if (journalEntryId == Guid.Empty)
            throw new BusinessRuleException("JournalEntryId is required.");

        if (lineNumber <= 0)
            throw new BusinessRuleException("Line number must be greater than zero.");

        if (accountId == Guid.Empty)
            throw new BusinessRuleException("AccountId is required.");

        if (debit < 0 || credit < 0)
            throw new BusinessRuleException("Amounts cannot be negative.");

        Id = Guid.NewGuid();
        JournalEntryId = journalEntryId;
        LineNumber = lineNumber;
        AccountId = accountId;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Debit = decimal.Round(debit, 2, MidpointRounding.AwayFromZero);
        Credit = decimal.Round(credit, 2, MidpointRounding.AwayFromZero);
    }

    public Guid JournalEntryId { get; private set; }
    public int LineNumber { get; private set; }
    public Guid AccountId { get; private set; }
    public string? Description { get; private set; }
    public decimal Debit { get; private set; }
    public decimal Credit { get; private set; }
    public JournalEntry JournalEntry { get; private set; } = default!;
    public Account Account { get; private set; } = default!;
}
