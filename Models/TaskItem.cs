using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManager.Models;

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
    public int Priority { get; set; } = 1; // Quanto menor o número, maior a prioridade
    public List<string> Tags { get; set; } = new List<string>();
    public string Category { get; set; } = "Geral";
    public bool HasAlarm { get; set; } = false;
    public DateTime? AlarmTime { get; set; }

    public bool IsRecurrent { get; set; } = false;
    public string? RecurrencePattern { get; set; } // Ex: "Diário", "Semanal", "Mensal"

    public List<int>? DependencyOnTaskIds { get; set; } = new List<int>(); // IDs de tarefas das quais esta tarefa depende
    public List<TaskItem>? DependsOnTasks { get; set; } = new List<TaskItem>(); // Tarefas das quais esta tarefa depende

    public string AuthorUserId { get; set; } = string.Empty; // ID do usuário que criou a tarefa
    public string? AssignedToUserId { get; set; } // ID do usuário ao qual a tarefa está atribuída
    [Range(0, 20, ErrorMessage = "O valor deve estar entre 0 e 20.")]
    public int NeedsReview { get; set; } = 0; // Quantidade de vezes que a tarefa precisa ser revisada antes de ser considerada completa

    public List<string>? ReviewByUserId { get; set; } // IDs dos usuários responsáveis por revisar a tarefa
    public List<string>? ReviewedByUserId { get; set; } // IDs dos usuários que já revisaram a tarefa
    public List<Comment>? Comments { get; set; } // Comentários relacionados à tarefa

}
