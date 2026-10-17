using Microsoft.EntityFrameworkCore;
using IntelSphere.API.Data;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. SERVICE REGISTRATION AREA (DI CONTAINER)
// ==========================================

// Registering the Database Connection (SQLite) to the application
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

// Adding Swagger/OpenAPI documentation services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ==========================================
// 2. HTTP REQUEST PIPELINE SETTINGS (MIDDLEWARE)
// ==========================================

// Enable the Swagger interface if the application is in Development mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "IntelSphere API v1");
    });

    // Redirect the root URL to the Swagger UI
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Exposes the entry point to the integration test project
public partial class Program { }