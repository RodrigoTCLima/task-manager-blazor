using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Components;
using TaskManager.Data;
using TaskManager.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ── DATABASE ─────────────────────────────────────────────────────────────────
// Development: SQLite  |  Production: PostgreSQL via Neon (DATABASE_URL)
var isDev = builder.Environment.IsDevelopment();

if (isDev)
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite("Data Source=tasks.db"));
}
else
{
    var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
        ?? throw new InvalidOperationException("DATABASE_URL is not set.");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(dbUrl));
}

// ── IDENTITY ──────────────────────────────────────────────────────────────────
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    // Lockout after 5 failed attempts for 5 minutes
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

    // Basic security headers
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
