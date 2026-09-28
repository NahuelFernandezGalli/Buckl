using Buckl.Api.Authentication;
using Buckl.Api.Errors;
using Buckl.Api.Filters;
using Buckl.Api.Json;
using Buckl.Application;
using Buckl.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddBucklAuthentication(builder.Configuration);
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = ProblemCodes.AddDefaultCode);
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services
    .AddControllers(options => options.Filters.Add<UserTransactionFilter>())
    .AddJsonOptions(options => BucklJson.Configure(options.JsonSerializerOptions));
builder.Services.ConfigureHttpJsonOptions(options => BucklJson.Configure(options.SerializerOptions));
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
}

app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllers();

app.Run();

/// <summary>Public so the integration tests can host the API with WebApplicationFactory.</summary>
public partial class Program;
