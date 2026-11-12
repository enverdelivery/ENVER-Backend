using Enver.API.Composition;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// OpenAPI
builder.Services.AddOpenApi();

// Health checks
builder.Services.AddHealthChecks();
builder.Services.AddAuthenticationFoundation(builder.Configuration, builder.Environment);
builder.Services.AddAuthenticationDependencies(builder.Configuration);
builder.Services.AddAuthenticationPipeline();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthenticationFoundation();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health endpoint
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();
