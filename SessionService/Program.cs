using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SessionService.Consumers;
using SessionService.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// builder.WebHost.UseUrls("http://localhost:5004");

builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri("http://elasticsearch:9200"))
    {
        AutoRegisterTemplate = true,
        IndexFormat = "session-logs-{0:yyyy.MM.dd}"
    })
    .CreateLogger();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ValidateDynamicIdConsumer>();
    x.AddConsumer<DynamicIdRegisteredConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("validate-dynamicid-request", e =>
        {
            e.ConfigureConsumer<ValidateDynamicIdConsumer>(context);
        });
        cfg.ReceiveEndpoint("dynamic-id-registered-event", e =>
        {
            e.ConfigureConsumer<DynamicIdRegisteredConsumer>(context);
        });
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();