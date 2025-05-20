using System.Security.Claims;
using System.Text;
using Registration.Data;
using Registration.Services;
using Registration.Clients;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Registration.Consumers;
using Registration.Models;
using Serilog;
using Shared.Messages;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = jwtSettings["Key"];
var issuer = jwtSettings["Issuer"];

builder.Host.UseSerilog();

builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<RegistrationAuthOptions>(
    builder.Configuration.GetSection("RegistrationAuth"));

builder.Services.AddScoped<IRegistrationService, RegistrationService>();

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri("http://elasticsearch:9200"))
    {
        AutoRegisterTemplate = true,
        IndexFormat = "registration-logs-{0:yyyy.MM.dd}"
    })
    .CreateLogger();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<RegistrationCheckInConsumer>();
    
    x.AddRequestClient<GetOrderRequest>();
    x.AddRequestClient<GetPassengerRequest>();
    x.AddRequestClient<GetPassengerBaggageWeightRequest>();
    x.AddRequestClient<ValidateDynamicIdRequest>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("registration-check-in-request", e =>
        {
            e.ConfigureConsumer<RegistrationCheckInConsumer>(context);
        });
        
    });
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

builder.Services.AddScoped<PassengerClient>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:8080",
                "http://localhost:8081",
                "http://localhost:8082",
                "http://localhost:8083",
                "http://localhost:8084",
                "http://localhost:3000",
                "http://localhost:3002")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT авторизация. Формат: Bearer {токен}",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim(ClaimTypes.Role, "admin"));
});

var app = builder.Build();

using (var s = app.Services.CreateScope())
{
    var db = s.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();