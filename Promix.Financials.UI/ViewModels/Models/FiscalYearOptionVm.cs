using System;

namespace Promix.Financials.UI.ViewModels.Accounts.Models;

public sealed class FiscalYearOptionVm
{
    public FiscalYearOptionVm(Guid id, string displayName)
    {
        Id = id;
        DisplayName = displayName;
    }

    public Guid Id { get; }
    public string DisplayName { get; }
}