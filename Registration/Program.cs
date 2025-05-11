using Registration.Data;
using Registration.Services;
using Registration.Clients;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Registration.Consumers;
using Registration.Models;
using Serilog;
using Shared.Messages;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddScoped<PassengerClient>();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

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

app.MapControllers();
app.Run();