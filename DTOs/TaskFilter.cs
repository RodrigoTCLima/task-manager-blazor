namespace TaskManager.DTOs;

public class TaskFilter
{
    public string? Category { get; set; }
    public string? Tag { get; set; }
    public bool? IsCompleted { get; set; }
    public string? DueDateRange { get; set; }
    public int? MaxPriority { get; set; }
    public string? SortBy { get; set; }
}
