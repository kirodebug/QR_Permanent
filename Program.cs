using DBPQRPermanent.Data;
using DBPQRPermanent.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Railway sets PORT env variable — must bind to it
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "DBP QR Permanent — Employee API",
        Version     = "v1",
        Description = "Manage employees and their permanent QR contact tokens."
    });
});

// SQLite — use /tmp on Railway (writable), local path in dev
var isProduction = (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production") == "Production";
var dbPath = isProduction ? "/tmp/dbpqr.db" : "dbpqr.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddScoped<QRService>();

var app = builder.Build();

// Create DB + seed — wrapped in try/catch so a seed error doesn't kill the app
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    DatabaseSeeder.Seed(db);
}
catch (Exception ex)
{
    Console.WriteLine($"[STARTUP] DB init error: {ex.Message}");
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DBP QR Permanent API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "DBP QR API";
});

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Health check endpoint — Railway uses this to verify app is running
app.MapGet("/health", () => Results.Ok(new { status = "ok", time = DateTime.UtcNow }));

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

Console.WriteLine($"[STARTUP] App running on port {port}");
app.Run();
