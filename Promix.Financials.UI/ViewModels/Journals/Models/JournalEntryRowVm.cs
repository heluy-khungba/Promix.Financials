using System;
using Promix.Financials.Domain.Enums;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Promix.Financials.UI.ViewModels.Journals.Models;

public sealed class JournalEntryRowVm
{
    public JournalEntryRowVm(
        Guid id,
        string entryNumber,
        DateOnly entryDate,
        JournalEntryType type,
        JournalEntryStatus status,
        string? referenceNo,
        string? description,
        decimal totalDebit,
        decimal totalCredit,
        int lineCount)
    {
        Id = id;
        EntryNumber = entryNumber;
        EntryDateText = entryDate.ToString("yyyy-MM-dd");
        Type = type;
        Status = status;
        ReferenceNo = string.IsNullOrWhiteSpace(referenceNo) ? "—" : referenceNo;
        Description = string.IsNullOrWhiteSpace(description) ? "بدون وصف إضافي" : description;
        TotalDebit = totalDebit;
        TotalCredit = totalCredit;
        LineCountText = $"{lineCount} سطر";
    }

    public Guid Id { get; }
    public string EntryNumber { get; }
    public string EntryDateText { get; }
    public JournalEntryType Type { get; }
    public JournalEntryStatus Status { get; }
    public string ReferenceNo { get; }
    public string Description { get; }
    public decimal TotalDebit { get; }
    public decimal TotalCredit { get; }
    public string LineCountText { get; }

    public bool IsDraft => Status == JournalEntryStatus.Draft;
    public bool IsBalanced => TotalDebit == TotalCredit && TotalDebit > 0;
    public string TotalDebitText => TotalDebit.ToString("N2");
    public string TotalCreditText => TotalCredit.ToString("N2");
    public string DifferenceText => Math.Abs(TotalDebit - TotalCredit).ToString("N2");

    public string TypeText => Type switch
    {
        JournalEntryType.ReceiptVoucher => "سند قبض",
        JournalEntryType.PaymentVoucher => "سند صرف",
        JournalEntryType.Adjustment => "قيد تسوية",
        _ => "قيد يومية"
    };

    public string TypeGlyph => Type switch
    {
        JournalEntryType.ReceiptVoucher => "\uE8C7",
        JournalEntryType.PaymentVoucher => "\uEAFD",
        JournalEntryType.Adjustment => "\uE777",
        _ => "\uE8A5"
    };

    public Brush TypeBackgroundBrush => Type switch
    {
        JournalEntryType.ReceiptVoucher => CreateBrush("#ECFDF5"),
        JournalEntryType.PaymentVoucher => CreateBrush("#FEF2F2"),
        JournalEntryType.Adjustment => CreateBrush("#FFF7ED"),
        _ => CreateBrush("#EFF6FF")
    };

    public Brush TypeForegroundBrush => Type switch
    {
        JournalEntryType.ReceiptVoucher => CreateBrush("#059669"),
        JournalEntryType.PaymentVoucher => CreateBrush("#DC2626"),
        JournalEntryType.Adjustment => CreateBrush("#EA580C"),
        _ => CreateBrush("#2563EB")
    };

    public string StatusText => Status == JournalEntryStatus.Posted ? "مرحل" : "مسودة";
    public Brush StatusBackgroundBrush => Status == JournalEntryStatus.Posted ? CreateBrush("#E0F2FE") : CreateBrush("#FEF3C7");
    public Brush StatusForegroundBrush => Status == JournalEntryStatus.Posted ? CreateBrush("#0369A1") : CreateBrush("#B45309");

    private static Brush CreateBrush(string hex)
    {
        var raw = hex.TrimStart('#');
        if (raw.Length == 6)
            raw = "FF" + raw;

        return new SolidColorBrush(Color.FromArgb(
            Convert.ToByte(raw[..2], 16),
            Convert.ToByte(raw.Substring(2, 2), 16),
            Convert.ToByte(raw.Substring(4, 2), 16),
            Convert.ToByte(raw.Substring(6, 2), 16)));
    }
}
