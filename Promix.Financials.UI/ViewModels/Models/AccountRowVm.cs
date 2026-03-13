namespace Promix.Financials.UI.ViewModels.Accounts.Models;

public sealed class AccountRowVm
{
    public AccountRowVm(
        string code,
        string arabicName,
        string typeText,
        string parentName,
        string currency,
        string role,
        bool isActive,
        string category)
    {
        Code = code;
        ArabicName = arabicName;
        TypeText = typeText;
        ParentName = parentName;
        Currency = currency;
        Role = role;
        IsActive = isActive;
        Category = category;
    }

    public string Code { get; }
    public string ArabicName { get; }
    public string TypeText { get; }   // Group / Postable
    public string ParentName { get; }
    public string Currency { get; }
    public string Role { get; }
    public bool IsActive { get; }
    public string Category { get; }   // Assets/Liabilities/...
}