using Microsoft.AspNetCore.Identity;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.Endpoints;

public static class AttachmentEndpoints
{
    public static void MapAttachmentEndpoints(this WebApplication app)
    {
        app.MapGet("/api/attachments/{id:int}/download", async (
            int id,
            HttpContext context,
            AttachmentService attachmentService,
            TaskService taskService,
            OrganizationService orgService,
            UserManager<ApplicationUser> userManager) =>
        {
            // 1. Verifica autenticação
            if (context.User.Identity?.IsAuthenticated != true)
                return Results.Unauthorized();

            var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            // 2. Busca o anexo e a task associada
            var attachment = await attachmentService.GetByIdAsync(id);
            if (attachment == null)
                return Results.NotFound();

            var task = await taskService.GetTaskByIdAsync(attachment.TaskItemId);
            if (task == null)
                return Results.NotFound();

            // 3. Verifica se o usuário tem acesso a essa task
            bool hasAccess;
            if (task.OrganizationId.HasValue)
            {
                var role = await orgService.GetUserRoleAsync(task.OrganizationId.Value, userId);
                hasAccess = role.HasValue; // qualquer membro da org pode ver anexos
            }
            else
            {
                hasAccess = task.AuthorUserId == userId
                    || (task.AssignedToUserIds?.Contains(userId) == true);
            }

            if (!hasAccess)
                return Results.Forbid();

            // 4. Busca o arquivo do R2 e retorna como stream
            var fileData = await attachmentService.GetFileStreamAsync(id);
            if (fileData == null)
                return Results.NotFound();

            return Results.File(fileData.Value.Stream, fileData.Value.ContentType, fileData.Value.FileName);
        })
        .RequireAuthorization();
    }
}