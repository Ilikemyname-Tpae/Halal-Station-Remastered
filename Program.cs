var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<UserStateUpdaterServices>();

builder.Services.AddControllers();

var app = builder.Build();

app.MapFallbackToFile("/pages/register.html");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();