using Scalar.AspNetCore;
using Service.Application;
using Service.Infrastructure;
using Service.Api.Features.Products;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var authAuthority = builder.Configuration["Authentication:Authority"];
var authAudience = builder.Configuration["Authentication:Audience"];
var authEnabled = !string.IsNullOrWhiteSpace(authAuthority) &&
    !string.IsNullOrWhiteSpace(authAudience);

if (authEnabled)
{
    builder.Services
        .AddAuthentication("Bearer")
        .AddJwtBearer("Bearer", options =>
        {
            options.Authority = authAuthority;
            options.Audience = authAudience;
            options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        });

    builder.Services.AddAuthorization();
}

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (authEnabled)
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/health/live", () => "OK");
app.MapGet("/health/ready", () => Results.Ok("READY"));
app.MapProductEndpoints();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";

app.Run($"http://0.0.0.0:{port}");
