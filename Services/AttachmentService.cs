using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.Services;

public class AttachmentService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _publicBaseUrl;

    // Limite de tamanho por arquivo: 20MB
    public const long MaxFileSizeBytes = 20 * 1024 * 1024;

    public AttachmentService(IDbContextFactory<AppDbContext> contextFactory, IConfiguration config)
    {
        _contextFactory = contextFactory;

        var accountId = config["R2_ACCOUNT_ID"] ?? Environment.GetEnvironmentVariable("R2_ACCOUNT_ID")
            ?? throw new InvalidOperationException("R2_ACCOUNT_ID não configurado.");
        var accessKey = config["R2_ACCESS_KEY_ID"] ?? Environment.GetEnvironmentVariable("R2_ACCESS_KEY_ID")
            ?? throw new InvalidOperationException("R2_ACCESS_KEY_ID não configurado.");
        var secretKey = config["R2_SECRET_ACCESS_KEY"] ?? Environment.GetEnvironmentVariable("R2_SECRET_ACCESS_KEY")
            ?? throw new InvalidOperationException("R2_SECRET_ACCESS_KEY não configurado.");
        _bucketName = config["R2_BUCKET_NAME"] ?? Environment.GetEnvironmentVariable("R2_BUCKET_NAME") ?? "taskmanager-attachments";
        _publicBaseUrl = config["R2_PUBLIC_URL"] ?? Environment.GetEnvironmentVariable("R2_PUBLIC_URL") ?? "";

        var s3Config = new AmazonS3Config
        {
            ServiceURL = $"https://{accountId}.r2.cloudflarestorage.com",
            ForcePathStyle = true
        };

        _s3Client = new AmazonS3Client(new BasicAWSCredentials(accessKey, secretKey), s3Config);
    }

    private AppDbContext CreateContext() => _contextFactory.CreateDbContext();

    public async Task<TaskAttachment> UploadAsync(int taskId, string fileName, string contentType, Stream fileStream, string uploadedByUserId)
    {
        var storageKey = $"tasks/{taskId}/{Guid.NewGuid()}_{fileName}";

        // Lê tudo para um array de bytes primeiro — evita problemas de stream sendo fechado
        // durante a leitura assíncrona interna do SDK do S3
        byte[] fileBytes;
        using (var ms = new MemoryStream())
        {
            await fileStream.CopyToAsync(ms);
            fileBytes = ms.ToArray();
        }

        using var uploadStream = new MemoryStream(fileBytes);

        var putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = storageKey,
            InputStream = uploadStream,
            ContentType = contentType,
            DisablePayloadSigning = true,
            AutoCloseStream = false
        };

        await _s3Client.PutObjectAsync(putRequest);

        var attachment = new TaskAttachment
        {
            TaskItemId = taskId,
            FileName = fileName,
            StorageKey = storageKey,
            ContentType = contentType,
            SizeBytes = fileBytes.Length,
            UploadedByUserId = uploadedByUserId,
            UploadedAt = DateTime.UtcNow
        };

        using var _context = CreateContext();
        _context.Set<TaskAttachment>().Add(attachment);
        await _context.SaveChangesAsync();

        return attachment;
    }

    public async Task<List<TaskAttachment>> GetByTaskIdAsync(int taskId)
    {
        using var _context = CreateContext();
        return await _context.Set<TaskAttachment>()
            .Where(a => a.TaskItemId == taskId)
            .OrderByDescending(a => a.UploadedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task DeleteAsync(int attachmentId)
    {
        using var _context = CreateContext();
        var attachment = await _context.Set<TaskAttachment>().FindAsync(attachmentId);
        if (attachment == null) return;

        try
        {
            await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = attachment.StorageKey
            });
        }
        catch
        {
            // Se falhar ao deletar do R2, ainda removemos do banco para não deixar referência quebrada
        }

        _context.Set<TaskAttachment>().Remove(attachment);
        await _context.SaveChangesAsync();
    }

    public async Task<(Stream Stream, string ContentType, string FileName)?> GetFileStreamAsync(int attachmentId)
    {
        using var _context = CreateContext();
        var attachment = await _context.Set<TaskAttachment>().FindAsync(attachmentId);
        if (attachment == null) return null;

        var response = await _s3Client.GetObjectAsync(_bucketName, attachment.StorageKey);
        return (response.ResponseStream, attachment.ContentType, attachment.FileName);
    }

    public async Task<TaskAttachment?> GetByIdAsync(int attachmentId)
    {
        using var _context = CreateContext();
        return await _context.Set<TaskAttachment>().AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == attachmentId);
    }
}
