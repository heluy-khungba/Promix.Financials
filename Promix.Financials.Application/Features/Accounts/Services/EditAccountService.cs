using Promix.Financials.Application.Abstractions;   // ✅ نفس namespace الـ Interface
using Promix.Financials.Application.Features.Accounts.Commands;
using Promix.Financials.Domain.Exceptions;

namespace Promix.Financials.Application.Features.Accounts.Services;

public sealed class EditAccountService
{
    private readonly IAccountRepository _repo;

    public EditAccountService(IAccountRepository repo)
        => _repo = repo;

    public async Task EditAsync(EditAccountCommand cmd)
    {
        var account = await _repo.GetByIdAsync(cmd.AccountId, cmd.CompanyId);

        if (account is null)
            throw new BusinessRuleException("الحساب غير موجود.");

        // ✅ القاعدة: لا تعديل على حسابات النظام
        if (account.SystemRole is not null)
            throw new BusinessRuleException("لا يمكن تعديل حساب النظام.");

        account.Update(cmd.ArabicName, cmd.EnglishName, cmd.IsActive, cmd.Notes);

        await _repo.SaveChangesAsync();
    }
}