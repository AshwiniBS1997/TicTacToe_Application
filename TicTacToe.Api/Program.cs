using System.Text.Json.Serialization;
using TicTacToe.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers + send enums as text ("InProgress") instead of numbers (0)
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Singletons = one shared instance for the whole app.
// This is what makes our in-memory storage live as long as the API is running.
builder.Services.AddSingleton<GameStore>();
builder.Services.AddSingleton<ScoreboardService>();
builder.Services.AddSingleton<ComputerPlayer>();
builder.Services.AddSingleton<GameService>();

// Allow the React dev server (Vite runs on port 5173) to call this API
const string ReactCorsPolicy = "ReactApp";
builder.Services.AddCors(options =>
{
    options.AddPolicy(ReactCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(ReactCorsPolicy);
app.MapControllers();

app.Run();

// Makes Program visible to test projects if integration tests are added later
public partial class Program { }
