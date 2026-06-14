namespace TaskManager.Services;

/// <summary>
/// Singleton service that broadcasts state changes across components.
/// Components subscribe to events and reload data when notified.
/// </summary>
public class AppState
{
    // Fired whenever tasks are created, updated, or deleted
    public event Action? OnTasksChanged;

    public void NotifyTasksChanged() => OnTasksChanged?.Invoke();
}
