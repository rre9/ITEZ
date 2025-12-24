using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace ITHelpDesk.Services;

public class TicketAttachmentService : ITicketAttachmentService
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".pdf"
    };

    private readonly IWebHostEnvironment _environment;

    public TicketAttachmentService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<TicketAttachmentMetadata> SaveAttachmentAsync(int ticketId, IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file is null)
        {
            throw new ArgumentNullException(nameof(file));
        }

        if (file.Length == 0)
        {
            throw new InvalidOperationException("The uploaded file is empty.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            throw new InvalidOperationException("The uploaded file exceeds the maximum allowed size of 10 MB.");
        }

        var extension = Path.GetExtension(file.FileName);

        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Unsupported file type. Only JPG, PNG, and PDF files are allowed.");
        }

        var uploadsFolder = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", ticketId.ToString());
        Directory.CreateDirectory(uploadsFolder);

        var storedFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var physicalPath = Path.Combine(uploadsFolder, storedFileName);

        await using var stream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await file.CopyToAsync(stream, cancellationToken);

        var relativePath = Path.Combine("uploads", ticketId.ToString(), storedFileName).Replace("\\", "/");
        var uploadedAt = DateTime.UtcNow;

        return new TicketAttachmentMetadata(
            OriginalFileName: file.FileName,
            StoredFileName: storedFileName,
            RelativePath: relativePath,
            Size: file.Length,
            UploadedAt: uploadedAt);
    }
}

