using SearchService.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

try
{
    await DbInitializer.InitDb(app);
} catch (Exception ex)
{
    Console.WriteLine($"Error initializing database: {ex.Message}");
}

app.Run();
