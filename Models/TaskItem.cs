using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManager.Models;

public enum KanbanStatus
{
    Todo = 0, // A fazer (padrão — valor 0 no banco)
    InProgress = 1, // Em andamento
    Done = 2, // Concluída
    Recurrent = 3  // Tarefas recorrentes
}

public class TaskItem
{
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;
    [MaxLength(500, ErrorMessage = "A descrição não pode exceder 500 caracteres.")]
    public string? Description { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "O valor deve ser maior ou igual a 1.")]
    public int Priority { get; set; } = 1;
    public List<string> Tags { get; set; } = new List<string>();
    public string Category { get; set; } = "Geral";
    public bool HasAlarm { get; set; } = false;
    public DateTime? AlarmTime { get; set; }

    public bool IsRecurrent { get; set; } = false;
    public string? RecurrencePattern { get; set; }

    /// <summary>Coluna do Kanban. Done sincroniza com IsCompleted.</summary>
    public KanbanStatus KanbanStatus { get; set; } = KanbanStatus.Todo;

    public List<int>? DependencyOnTaskIds { get; set; } = new List<int>();
    public List<TaskItem>? DependsOnTasks { get; set; } = new List<TaskItem>();

    public string AuthorUserId { get; set; } = string.Empty;
    public List<string> AssignedToUserIds { get; set; } = new();
    public int? OrganizationId { get; set; }

    [Range(0, 20, ErrorMessage = "O valor deve estar entre 0 e 20.")]
    public int NeedsReview { get; set; } = 0;

    public List<string>? ReviewByUserId { get; set; }
    public List<string>? ReviewedByUserId { get; set; }
    public List<Comment>? Comments { get; set; }


    /// <summary>ID da task original da série recorrente (null se for a original)</summary>
    public int? ParentTaskId { get; set; }

    /// <summary>True se esta task foi gerada automaticamente por recorrência</summary>
    public bool IsRecurrentInstance { get; set; } = false;
}
