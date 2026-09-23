using Buckl.Api;
using Buckl.Application;
using Buckl.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddScoped<Buckl.Application.Abstractions.ICurrentUser, StubCurrentUser>();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();

/// <summary>Public so the integration tests can host the API with WebApplicationFactory.</summary>
public partial class Program;
