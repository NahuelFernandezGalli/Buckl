using Buckl.Api.Authentication;
using Buckl.Api.Filters;
using Buckl.Application;
using Buckl.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddBucklAuthentication(builder.Environment);
builder.Services.AddControllers(options => options.Filters.Add<UserTransactionFilter>());
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllers();

app.Run();

/// <summary>Public so the integration tests can host the API with WebApplicationFactory.</summary>
public partial class Program;
