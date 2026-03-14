using Promix.Financials.Application.Abstractions;
using Promix.Financials.Application.Features.Accounts.Commands;
using Promix.Financials.Domain.Aggregates.Accounts;
using Promix.Financials.Domain.Exceptions;

namespace Promix.Financials.Application.Features.Accounts.Services;

public sealed class CreateAccountService
{
    private readonly IAccountRepository _accounts;

    public CreateAccountService(IAccountRepository accounts)
    {
        _accounts = accounts;
    }

    public async Task<CreateAccountResult> CreateAsync(CreateAccountCommand cmd)
    {
        if (cmd.CompanyId == Guid.Empty)
            throw new BusinessRuleException("CompanyId is required.");

        var code = cmd.Code?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(code))
            throw new BusinessRuleException("Account code is required.");

        var nameAr = cmd.ArabicName?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(nameAr))
            throw new BusinessRuleException("Arabic name is required.");

        if (await _accounts.CodeExistsAsync(cmd.CompanyId, code))
            throw new BusinessRuleException("Account code already exists.");

        if (!string.IsNullOrWhiteSpace(cmd.SystemRole))
        {
            var role = cmd.SystemRole.Trim();
            if (await _accounts.SystemRoleExistsAsync(cmd.CompanyId, role))
                throw new BusinessRuleException("System role already assigned in this company.");
        }

        // Parent validation
        if (cmd.ParentId is not null)
        {
            var parent = await _accounts.GetByIdAsync(cmd.ParentId.Value);
            if (parent is null)
                throw new BusinessRuleException("Parent account not found.");

            if (parent.CompanyId != cmd.CompanyId)
                throw new BusinessRuleException("Parent account belongs to another company.");

            if (parent.IsPosting)
                throw new BusinessRuleException("Cannot add child under a postable account.");
        }

        var account = new Account(
            companyId: cmd.CompanyId,
            code: code,
            nameAr: nameAr,
            nameEn: cmd.EnglishName,
            nature: cmd.Nature,
            isPosting: cmd.IsPosting,
            parentId: cmd.ParentId,
            currencyCode: cmd.CurrencyCode,
            systemRole: cmd.SystemRole,
            notes: cmd.Notes,
            isActive: cmd.IsActive
        );

        await _accounts.AddAsync(account);
        await _accounts.SaveChangesAsync();

        return new CreateAccountResult(account.Id);
    }
}