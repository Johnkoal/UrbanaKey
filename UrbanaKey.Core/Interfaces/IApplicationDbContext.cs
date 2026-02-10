using Microsoft.EntityFrameworkCore;
using UrbanaKey.Core.Domain;

namespace UrbanaKey.Core.Interfaces;

public interface IApplicationDbContext
{
    DbSet<PQRS> PQRS { get; }
    DbSet<User> Users { get; } // Likely needed for User-related queries
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
