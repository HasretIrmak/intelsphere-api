using Microsoft.EntityFrameworkCore;
using IntelSphere.API.Data;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. SERVÝSLERÝ KAYDETME ALANI (DI CONTAINER)
// ==========================================

// Veritabaný Baðlantýsýný (SQLite) uygulamaya tanýtýyoruz
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

// Swagger/OpenAPI dökümantasyon servislerini ekliyoruz
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ==========================================
// 2. HTTP ÝSTEK HATTI AYARLARI (MIDDLEWARE)
// ==========================================

// Eðer uygulama geliþtirme modundaysa (Development), Swagger arayüzünü aktif et
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "IntelSphere API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();