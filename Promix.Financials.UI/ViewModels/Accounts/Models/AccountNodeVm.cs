using System;
using System.Collections.ObjectModel;

namespace Promix.Financials.UI.ViewModels.Accounts.Models;

public sealed class AccountNodeVm
{
    public AccountNodeVm(
        Guid id,
        string code,
        string arabicName,
        bool isPosting,     // true = Postable (حركي), false = Group (تجميعي)
        bool isSystem,
        bool isActive,
        Guid? parentId = null)
    {
        Id = id;
        ParentId = parentId;

        Code = code;
        ArabicName = arabicName;

        IsPosting = isPosting;
        IsSystem = isSystem;
        IsActive = isActive;
    }

    public Guid Id { get; }
    public Guid? ParentId { get; }

    public string Code { get; }
    public string ArabicName { get; }

    public bool IsPosting { get; }   // Postable
    public bool IsSystem { get; }
    public bool IsActive { get; }

    // للواجهة الحالية (Converters تعتمد على TypeText)
    public string TypeText => IsPosting ? "Postable" : "Group";

    public ObservableCollection<AccountNodeVm> Children { get; } = new();
    public bool HasChildren => Children.Count > 0;
}