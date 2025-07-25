using joblywebapi.Helpers;
using JoblyWebApi.Interface;
using JoblyWebApi.Repositories;
using JoblyWebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ✅ 1. Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<NaukriLoginService>();

builder.Services.AddScoped<IUnitOfWork>(sp =>
    new UnitOfWork(DbConnectionHelper.ConnectionString));
LinkedInJobHelper.Init(builder.Configuration);
builder.Services.AddScoped<joblywebapi.Services.LinkedInService>();

// Program.cs
builder.Services.AddScoped<Func<int, IGroqService>>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    return userId =>
    {
        var uow = sp.GetRequiredService<IUnitOfWork>();
        return new GroqService(cfg["Groq:ApiKey"]!, userId, uow);
    };
});

// ✅ Swagger config
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "JoblyWebApi", Version = "v1" });

    c.OperationFilter<FormFileOperationFilter>(); // For IFormFile

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT token (format: Bearer {token})",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ✅ JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

var app = builder.Build();

// ✅ 3. Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();  // 👈 BEFORE authorization
app.UseAuthorization();

app.MapControllers();

app.Run();
