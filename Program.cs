using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TaskManager.Components;
using TaskManager.Data;
using TaskManager.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DisconnectedCircuitMaxRetained = 100;
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(5);
        options.JSInteropDefaultCallTimeout = TimeSpan.FromSeconds(30);
    });

// ── DATABASE ──────────────────────────────────────────────────────────────────
var isDev = builder.Environment.IsDevelopment();

if (isDev)
{
    builder.Services.AddDbContextFactory<AppDbContext>(options =>
        options
            .UseSqlite("Data Source=tasks.db")
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));
    builder.Services.AddScoped<AppDbContext>(p =>
        p.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());
}
else
{
    var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
        ?? throw new InvalidOperationException("DATABASE_URL is not set.");

    builder.Services.AddDbContextFactory<AppDbContext>(options =>
        options
            .UseNpgsql(dbUrl)
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));
    builder.Services.AddScoped<AppDbContext>(p =>
        p.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());
}

// ── IDENTITY ──────────────────────────────────────────────────────────────────
builder.Services.AddDefaultIdentity<TaskManager.Models.ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = !isDev;
    options.SignIn.RequireConfirmedEmail = !isDev;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddCascadingAuthenticationState();

// ── APP SERVICES ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<OrganizationService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddSingleton<AppState>();

var app = builder.Build();

// ── MIDDLEWARE ────────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();

    app.Use(async (context, next) =>
    {
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
        await next();
    });
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    var allowedPaths = new[]
    {
        "/Identity/Account/EmailNotConfirmed",
        "/Identity/Account/ResendEmailConfirmation",
        "/Identity/Account/ConfirmEmail",
        "/Identity/Account/Login",
        "/Identity/Account/Register",
        "/Identity/Account/RegisterConfirmation",
        "/Identity/Account/Logout",
        "/Identity/Account/ForgotPassword",
        "/Identity/Account/ForgotPasswordConfirmation",
        "/Identity/Account/ResetPassword",
        "/Identity/Account/ResetPasswordConfirmation",
        "/_blazor",
        "/_framework",
        "/css",
        "/lib",
        "/js"
    };

    var path = context.Request.Path.Value ?? "";
    var isAllowedPath = allowedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));

    if (!isAllowedPath && context.User.Identity?.IsAuthenticated == true)
    {
        var userManager = context.RequestServices.GetRequiredService<UserManager<TaskManager.Models.ApplicationUser>>();
        var user = await userManager.GetUserAsync(context.User);

        if (user != null && !user.EmailConfirmed && !isDev)
        {
            context.Response.Redirect("/Identity/Account/EmailNotConfirmed");
            return;
        }
    }

    await next();
});

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapRazorPages();

// ── AUTO MIGRATE ON STARTUP ───────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}

app.Run();