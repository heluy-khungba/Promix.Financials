using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Promix.Financials.Application.Abstractions;
using Promix.Financials.Domain.Security;
using Promix.Financials.Infrastructure.Persistence;

namespace Promix.Financials.Infrastructure.Security;

public sealed class EfUserRepository : IUserRepository
{
    private readonly PromixDbContext _db;

    public EfUserRepository(PromixDbContext db)
    {
        _db = db;
    }

    public Task<User?> FindByUsernameAsync(string username, CancellationToken ct = default)
        => _db.Users.FirstOrDefaultAsync(u => u.Username == username, ct);

    public Task<User?> FindByIdAsync(Guid userId, CancellationToken ct = default)
        => _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
}