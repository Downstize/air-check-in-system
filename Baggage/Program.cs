using Baggage.Clients;
using Baggage.Consumers;
using Baggage.Data;
using Baggage.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Shared.Messages;

var builder = WebApplication.CreateBuilder(args);

// builder.WebHost.UseUrls("http://localhost:5001");

builder.Host.UseSerilog();

builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IBaggageService, BaggageService>();

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri("http://elasticsearch:9200"))
    {
        AutoRegisterTemplate = true,
        IndexFormat = "baggage-logs-{0:yyyy.MM.dd}"
    })
    .CreateLogger();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<BaggageRegistrationConsumer>();
    x.AddConsumer<GetPassengerBaggageWeightConsumer>();
    
    x.AddRequestClient<GetOrderRequest>();
    x.AddRequestClient<ValidateDynamicIdRequest>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("baggage-registration-request", e =>
        {
            e.ConfigureConsumer<BaggageRegistrationConsumer>(context);
        });
        cfg.ReceiveEndpoint("get-passenger-baggage-weight-request", e =>
        {
            e.ConfigureConsumer<GetPassengerBaggageWeightConsumer>(context);
        });
    });
});

builder.Services.AddScoped<PassengerClient>();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    DbInitializer.Seed(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();