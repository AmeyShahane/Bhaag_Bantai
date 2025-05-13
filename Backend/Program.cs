using System.Text.Json.Serialization;
using Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
                //.AddJsonOptions(options =>
                //{
                //    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
                //    options.JsonSerializerOptions.WriteIndented = true;
                //});
builder.Services.AddSingleton<MapService>();
builder.Services.AddSingleton<GameService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
        policy.WithOrigins("http://localhost:3000") // React dev server
              .AllowAnyHeader()
              .AllowAnyMethod());
});




var app = builder.Build();
_ = app.Services.GetRequiredService<GameService>();
app.UseCors("AllowReactApp");

//app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapControllers();

app.Run();
