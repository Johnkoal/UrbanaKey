using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanaKey.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UrbanaKey.Core.Features.Admin;

public record AuditLogResponse(Guid Id, string EntityName, string Action, string Changes, string UserId, DateTime Timestamp);
public record GetAuditLogsQuery() : IRequest<List<AuditLogResponse>>;

public class GetAuditLogsHandler(IApplicationDbContext db) : IRequestHandler<GetAuditLogsQuery, List<AuditLogResponse>>
{
    public async Task<List<AuditLogResponse>> Handle(GetAuditLogsQuery request, CancellationToken ct)
    {
        return await db.AuditLogs
            .OrderByDescending(a => a.Timestamp)
            .Select(a => new AuditLogResponse(
                a.Id, 
                a.EntityName, 
                a.Action, 
                $"Old: {a.OldValues}, New: {a.NewValues}", // Combine for simplicity
                a.UserId, 
                a.Timestamp))
            .ToListAsync(ct);
    }
}
