using System;
using System.Collections.Generic;
using System.IO;

namespace UrbanaKey.Core.Features.PQRS;

public record CreatePqrRequest(
    string Title, 
    string Description, 
    Guid UnitId, 
    bool IsPublic, 
    string? AttachmentUrl = null,
    List<Stream>? Attachments = null);

public record PqrResponse(
    Guid Id, 
    string Title, 
    string Description, 
    string Status, 
    DateTime CreatedAt, 
    bool IsPublic, 
    string? AttachmentUrl,
    List<string> AttachmentUrls);
