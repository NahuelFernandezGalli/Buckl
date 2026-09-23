using Buckl.Application;
using Buckl.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();

/// <summary>Public so the integration tests can host the API with WebApplicationFactory.</summary>
public partial class Program;
