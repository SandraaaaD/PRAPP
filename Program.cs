using System.Text;
using BenchApp.Data;
using BenchApp.Middleware;
using BenchApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// EF Core - connects to your existing PostgreSQL database.
// Update the "DefaultConnection" value in appsettings.json (or appsettings.Development.json)
// with your real host/port/database/username/password.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SANDRA
// Make [ApiController]'s automatic model-validation (from data annotations on the DTOs)
// respond with our shared ApiErrorResponse contract instead of the ASP.NET Core default
// ValidationProblemDetails shape. This is what makes 2.1 work for every endpoint for free.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var fieldErrors = context.ModelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .SelectMany(kvp => kvp.Value!.Errors.Select(e => new FieldError
            {
                Field = ToCamelCase(kvp.Key),
                Message = e.ErrorMessage
            }))
            .ToList();

        var response = new ApiErrorResponse
        {
            Code = ErrorCodes.ValidationError,
            Message = "One or more fields are invalid.",
            FieldErrors = fieldErrors,
            Retryable = false
        };

        return new BadRequestObjectResult(response);
    };
});

var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

var allowedOrigin = builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:5173";
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// NOTE: we intentionally do NOT call db.Database.Migrate() here.
// Running it automatically at startup executes during "dotnet ef migrations add" too
// (since this is plain sequential code), which needs the database to actually be
// reachable and can produce confusing errors while you're just generating a migration.
// Instead, apply migrations explicitly and only once with:
//   dotnet ef database update
// Seeding reference data is safe to leave automatic - it only inserts rows if the
// tables are empty, and by the time you're running the app, migrations should
// already be applied.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    SeedData.Initialize(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// SANDRA - global error handler must be first in the pipeline so it can catch
// anything thrown further down (auth, controllers, EF, etc.)
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static string ToCamelCase(string value)
{
    if (string.IsNullOrEmpty(value)) return value;
    return char.ToLowerInvariant(value[0]) + value[1..];
}
