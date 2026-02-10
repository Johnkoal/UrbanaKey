using System;

namespace UrbanaKey.Core.Features.Residents;

public record LinkResidentRequest(Guid UserId, Guid UnitId, string LinkType, bool IsResponsible);

public record ResidentProfileResponse(Guid ProfileId, string FullName, string Email, string LinkType, bool IsResponsible);
