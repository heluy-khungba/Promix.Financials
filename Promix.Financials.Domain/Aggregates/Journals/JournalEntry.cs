using Promix.Financials.Domain.Common;
using Promix.Financials.Domain.Enums;
using Promix.Financials.Domain.Exceptions;

namespace Promix.Financials.Domain.Aggregates.Journals;

public sealed class JournalEntry : AggregateRoot<Guid>
{
    private readonly List<JournalLine> _lines = new();

    private JournalEntry() { }

    public JournalEntry(
        Guid companyId,
        string entryNumber,
        DateOnly entryDate,
        JournalEntryType type,
        Guid createdByUserId,
        DateTimeOffset createdAtUtc,
        string? referenceNo,
        string? description)
    {
        if (companyId == Guid.Empty)
            throw new BusinessRuleException("CompanyId is required.");

        if (createdByUserId == Guid.Empty)
            throw new BusinessRuleException("CreatedByUserId is required.");

        if (string.IsNullOrWhiteSpace(entryNumber))
            throw new BusinessRuleException("Entry number is required.");

        Id = Guid.NewGuid();
        CompanyId = companyId;
        EntryNumber = entryNumber.Trim().ToUpperInvariant();
        EntryDate = entryDate;
        Type = type;
        Status = JournalEntryStatus.Draft;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = createdAtUtc;
        ReferenceNo = Normalize(referenceNo, 50);
        Description = Normalize(description, 500);
    }

    public Guid CompanyId { get; private set; }
    public string EntryNumber { get; private set; } = default!;
    public DateOnly EntryDate { get; private set; }
    public JournalEntryType Type { get; private set; }
    public JournalEntryStatus Status { get; private set; }
    public string? ReferenceNo { get; private set; }
    public string? Description { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public Guid? PostedByUserId { get; private set; }
    public DateTimeOffset? PostedAtUtc { get; private set; }

    public IReadOnlyCollection<JournalLine> Lines => _lines.AsReadOnly();

    public decimal TotalDebit => _lines.Sum(x => x.Debit);
    public decimal TotalCredit => _lines.Sum(x => x.Credit);
    public bool IsBalanced => TotalDebit == TotalCredit && TotalDebit > 0;

    public void AddLine(Guid accountId, string? description, decimal debit, decimal credit)
    {
        EnsureDraft();

        var normalizedDebit = decimal.Round(debit, 2, MidpointRounding.AwayFromZero);
        var normalizedCredit = decimal.Round(credit, 2, MidpointRounding.AwayFromZero);

        if (accountId == Guid.Empty)
            throw new BusinessRuleException("Account is required.");

        if (normalizedDebit < 0 || normalizedCredit < 0)
            throw new BusinessRuleException("Debit and credit must be positive values.");

        var hasDebit = normalizedDebit > 0;
        var hasCredit = normalizedCredit > 0;

        if (hasDebit == hasCredit)
            throw new BusinessRuleException("Each line must contain either debit or credit.");

        _lines.Add(new JournalLine(
            journalEntryId: Id,
            lineNumber: _lines.Count + 1,
            accountId: accountId,
            description: description,
            debit: normalizedDebit,
            credit: normalizedCredit));
    }

    public void Post(Guid postedByUserId, DateTimeOffset postedAtUtc)
    {
        EnsureDraft();

        if (postedByUserId == Guid.Empty)
            throw new BusinessRuleException("PostedByUserId is required.");

        if (_lines.Count < 2)
            throw new BusinessRuleException("The journal entry must contain at least two lines.");

        if (!IsBalanced)
            throw new BusinessRuleException("The journal entry is not balanced.");

        Status = JournalEntryStatus.Posted;
        PostedByUserId = postedByUserId;
        PostedAtUtc = postedAtUtc;
    }

    private void EnsureDraft()
    {
        if (Status == JournalEntryStatus.Posted)
            throw new BusinessRuleException("Posted journal entries cannot be modified.");
    }

    private static string? Normalize(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
