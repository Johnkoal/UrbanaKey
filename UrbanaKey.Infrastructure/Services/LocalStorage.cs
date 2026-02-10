using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using UrbanaKey.Core.Interfaces;

namespace UrbanaKey.Infrastructure.Services;

public class LocalStorage(IWebHostEnvironment env) : IFileStorage
{
    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string module, Guid tenantId)
    {
        // Path: wwwroot/uploads/{tenantId}/{module}/{fileName}
        var uploadsFolder = Path.Combine(env.WebRootPath ?? "wwwroot", "uploads", tenantId.ToString(), module);
        
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var filePath = Path.Combine(uploadsFolder, fileName);
        
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(stream);
        }

        // Return relative path for web access
        return $"/uploads/{tenantId}/{module}/{fileName}";
    }
}
