using System;

namespace Promix.Financials.UI.ViewModels.Journals.Models;

public sealed record JournalAccountOptionVm(Guid Id, string Code, string NameAr)
{
    public string DisplayText => $"{Code} - {NameAr}";
}
