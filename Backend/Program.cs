using Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<MapService>();


var app = builder.Build();


//app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapControllers();

app.Run();
