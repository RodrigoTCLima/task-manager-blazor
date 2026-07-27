using System;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models;

public class TaskAttachment
{
    public int Id { get; set; }

    public int TaskItemId { get; set; }

    [Required]
    public string FileName { get; set; } = string.Empty;

    /// <summary>Chave (path) do objeto no bucket R2 — usada para deletar/gerar URL</summary>
    [Required]
    public string StorageKey { get; set; } = string.Empty;

    /// <summary>Content-Type do arquivo (ex: application/pdf, image/png)</summary>
    public string ContentType { get; set; } = "application/octet-stream";

    /// <summary>Tamanho em bytes</summary>
    public long SizeBytes { get; set; }

    public string UploadedByUserId { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
