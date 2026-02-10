using Microsoft.EntityFrameworkCore;
using UrbanaKey.Core.Domain;

namespace UrbanaKey.Core.Interfaces;

public interface IApplicationDbContext
{
    DbSet<PQRS> PQRS { get; }
    DbSet<User> Users { get; } 
    DbSet<Assembly> Assemblies { get; }
    DbSet<Vote> Votes { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
