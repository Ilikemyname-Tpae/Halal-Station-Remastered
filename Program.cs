var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

app.MapFallbackToFile("/pages/register.html");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();