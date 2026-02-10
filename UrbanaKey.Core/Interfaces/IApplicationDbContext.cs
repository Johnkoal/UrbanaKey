using Microsoft.EntityFrameworkCore;
using UrbanaKey.Core.Domain;

namespace UrbanaKey.Core.Interfaces;

public interface IApplicationDbContext
{
    DbSet<PQRS> PQRS { get; }
    DbSet<User> Users { get; } 
    DbSet<Assembly> Assemblies { get; }
    DbSet<Vote> Votes { get; }
    DbSet<Unit> Units { get; }
    DbSet<ResidentProfile> ResidentProfiles { get; }
    DbSet<Sanction> Sanctions { get; }
    DbSet<CommonArea> CommonAreas { get; }
    DbSet<AmenityBooking> AmenityBookings { get; }
    DbSet<PqrComment> PqrComments { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<Payment> Payments { get; }
    DbSet<AuditLog> AuditLogs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
