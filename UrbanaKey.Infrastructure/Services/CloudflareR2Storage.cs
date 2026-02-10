using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using UrbanaKey.Core.Interfaces;

namespace UrbanaKey.Infrastructure.Services;

public class CloudflareR2Storage : IFileStorage
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public CloudflareR2Storage(IAmazonS3 s3Client, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _bucketName = configuration["Storage:BucketName"] ?? "urbanakey-assets";
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string module, Guid tenantId)
    {
        // Path: {tenantId}/{module}/{fileName}
        var key = $"{tenantId}/{module}/{fileName}";

        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = fileStream,
            Key = key,
            BucketName = _bucketName,
            CannedACL = S3CannedACL.Private // Or PublicRead based on requirement
        };

        var fileTransferUtility = new TransferUtility(_s3Client);
        await fileTransferUtility.UploadAsync(uploadRequest);

        // Return URL (Public or Presigned?) 
        // SSoT doesn't specify logic for retrieval, assuming standard URL format or CDN.
        // If Cloudflare, usually https://<account>.r2.cloudflarestorage.com/<bucket>/<key> OR custom domain.
        // I'll return the Key for now or a formatted URL if config provides base URL.
        
        return key; 
    }
}
