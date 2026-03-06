using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManager.Models;

public class Comment
{
    public int Id { get; set; }
    public string AuthorUserId { get; set; } = string.Empty; // ID do usuário que fez o comentário
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int TaskItemId { get; set; } // ID da tarefa à qual o comentário pertence
}
