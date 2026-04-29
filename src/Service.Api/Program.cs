using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Service.Api.Common;
using Service.Api.Features.Auth;
using Service.Api.Features.Customers;
using Service.Api.Features.Orders;
using Service.Api.OpenApi;
using Scalar.AspNetCore;
using Service.Application;
using Service.Infrastructure;
using Service.Api.Features.Products;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "Service.Api");
});

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<TokenUserOptions>(
    builder.Configuration.GetSection(TokenUserOptions.SectionName));
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddSingleton<IRefreshTokenStore, InMemoryRefreshTokenStore>();

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>() ?? new JwtOptions();

var signingKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(jwtOptions.SigningKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("fixed", opt =>
    {
        var rateLimitConfig = builder.Configuration.GetSection("RateLimit");
        opt.PermitLimit = rateLimitConfig.GetValue<int>("PermitLimit", 100);
        opt.Window = TimeSpan.FromMinutes(rateLimitConfig.GetValue<int>("WindowMinutes", 1));
        opt.QueueLimit = rateLimitConfig.GetValue<int>("QueueLimit", 0);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Service.Infrastructure.Persistence.AppDbContext>();
    await Service.Infrastructure.DbInitializer.SeedAsync(context);
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        var correlationId = httpContext.GetCorrelationId();

        if (!string.IsNullOrWhiteSpace(correlationId))
            diagnosticContext.Set("CorrelationId", correlationId);
    };
});

app.UseCors();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/health/live", () => "OK");
app.MapGet("/health/ready", async (
    Service.Infrastructure.Persistence.AppDbContext db,
    CancellationToken ct) =>
                            {
                                var canConnect = await db.Database.CanConnectAsync(ct);

                                return canConnect
                                    ? Results.Ok("READY")
                                    : Results.Problem(
                                        title: "Database is not ready",
                                        statusCode: StatusCodes.Status503ServiceUnavailable);
                            });
app.MapAuthEndpoints();
app.MapProductEndpoints();
app.MapCustomerEndpoints();
app.MapOrderEndpoints();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";

app.Run($"http://0.0.0.0:{port}");
