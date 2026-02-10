using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanaKey.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UrbanaKey.Core.Features.Amenities;

public record UpdateCommonAreaCommand(Guid Id, CreateCommonAreaRequest Request, bool IsActive) : IRequest<bool>;

public class UpdateCommonAreaHandler(IApplicationDbContext db) : IRequestHandler<UpdateCommonAreaCommand, bool>
{
    public async Task<bool> Handle(UpdateCommonAreaCommand command, CancellationToken ct)
    {
        var area = await db.CommonAreas.FirstOrDefaultAsync(a => a.Id == command.Id, ct);
        if (area == null) return false;

        area.Name = command.Request.Name;
        area.Description = command.Request.Description;
        area.Capacity = command.Request.Capacity;
        area.HourlyRate = command.Request.HourlyRate;
        area.IsActive = command.IsActive;

        await db.SaveChangesAsync(ct);
        return true;
    }
}
