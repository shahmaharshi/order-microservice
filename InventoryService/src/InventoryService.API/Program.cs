using Azure.Messaging.ServiceBus;
using InventoryService.Application.Handlers;
using InventoryService.Application.Interfaces;
using InventoryService.Infrastructure.Messaging;
using InventoryService.Infrastructure.Persistence;
using InventoryService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<OrderCreatedEventHandler>();

var sbConn = builder.Configuration["AzureServiceBus:ConnectionString"]!;
builder.Services.AddSingleton(new ServiceBusClient(sbConn));
builder.Services.AddSingleton<IMessagePublisher>(_ => new AzureServiceBusPublisher(sbConn));
builder.Services.AddHostedService<InventoryServiceBusConsumer>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
