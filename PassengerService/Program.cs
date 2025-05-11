using Microsoft.EntityFrameworkCore;
using PassengerService.Data;
using MassTransit;
using PassengerService.Consumers;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri("http://elasticsearch:9200"))
    {
        AutoRegisterTemplate = true,
        IndexFormat = "passenger-logs-{0:yyyy.MM.dd}"
    })
    .CreateLogger();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<GetOrderConsumer>();
    x.AddConsumer<GetPassengerConsumer>();
    x.AddConsumer<PassengerStatusUpdatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        
        cfg.ReceiveEndpoint("get-order-request", e =>
        {
            e.ConfigureConsumer<GetOrderConsumer>(context);
        });
        
        cfg.ReceiveEndpoint("get-passenger-request", e =>
        {
            e.ConfigureConsumer<GetPassengerConsumer>(context);
        });
        cfg.ReceiveEndpoint("passenger-status-updated", e =>
        {
            e.ConfigureConsumer<PassengerStatusUpdatedConsumer>(context);
        });
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    DbInitializer.Seed(db);
}

app.Run();