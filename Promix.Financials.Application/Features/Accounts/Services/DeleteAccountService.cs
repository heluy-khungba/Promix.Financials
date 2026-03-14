using Promix.Financials.Application.Abstractions;   // ✅ نفس namespace الـ Interface
using Promix.Financials.Domain.Exceptions;

namespace Promix.Financials.Application.Features.Accounts.Services;

public sealed class DeleteAccountService
{
    private readonly IAccountRepository _repo;

    public DeleteAccountService(IAccountRepository repo)
        => _repo = repo;

    public async Task DeleteAsync(Guid accountId, Guid companyId)
    {
        var account = await _repo.GetByIdAsync(accountId, companyId);

        if (account is null)
            throw new BusinessRuleException("الحساب غير موجود.");

        // ✅ القاعدة 1: حسابات النظام لا تُحذف
        if (account.SystemRole is not null)
            throw new BusinessRuleException("لا يمكن حذف حساب النظام.");

        // ✅ القاعدة 2: لا حذف إذا له أبناء
        if (await _repo.HasChildrenAsync(accountId, companyId))
            throw new BusinessRuleException(
                "لا يمكن حذف الحساب لأنه يحتوي على حسابات فرعية.\nاحذف الحسابات الفرعية أولاً.");

        // ✅ القاعدة 3: لا حذف إذا له حركات
        if (await _repo.HasMovementsAsync(accountId, companyId))
            throw new BusinessRuleException(
                "لا يمكن حذف الحساب لأنه يحتوي على حركات محاسبية مسجّلة.");

        _repo.Remove(account);
        await _repo.SaveChangesAsync();
    }
}