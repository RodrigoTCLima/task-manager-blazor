using Microsoft.AspNetCore.Identity;

namespace TaskManager.Models;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }

    /// <summary>Nome amigável para exibição: usa DisplayName se definido, senão a parte antes do @ do email</summary>
    public string GetDisplayName() =>
        !string.IsNullOrWhiteSpace(DisplayName)
            ? DisplayName
            : (Email?.Split('@')[0] ?? UserName ?? "Usuário");
}