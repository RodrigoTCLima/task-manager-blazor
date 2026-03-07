using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Services;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=tasks.db"));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<CommentService>();

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();  // ← IMPORTANTE
app.UseAuthorization();   // ← IMPORTANTE

app.UseAntiforgery();

app.MapRazorComponents<TaskManager.Components.App>()
    .AddInteractiveServerRenderMode();

app.MapRazorPages(); // ← substitui MapIdentity

// SEED USUÁRIO INICIAL
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    context.Database.Migrate();
    
    if (!userManager.Users.Any())
    {
        var user = new IdentityUser 
        { 
            UserName = "admin@test.com", 
            Email = "admin@test.com" 
        };
        var result = userManager.CreateAsync(user, "Admin123!").Result;
    }
}

app.Run();
