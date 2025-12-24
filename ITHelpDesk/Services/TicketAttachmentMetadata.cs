using System;

namespace ITHelpDesk.Services;

public record TicketAttachmentMetadata(
    string OriginalFileName,
    string StoredFileName,
    string RelativePath,
    long Size,
    DateTime UploadedAt);

