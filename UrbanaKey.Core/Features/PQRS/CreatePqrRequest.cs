using System;

namespace UrbanaKey.Core.Features.PQRS;

public record CreatePqrRequest(string Title, string Description, Guid UnitId, bool IsPublic, string? AttachmentUrl);
public record PqrResponse(Guid Id, string Title, string Description, string Status, DateTime CreatedAt, bool IsPublic, string? AttachmentUrl);
