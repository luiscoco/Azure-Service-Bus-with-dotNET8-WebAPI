using AzureServiceBus.Model;

var builder = WebApplication.CreateBuilder(args);

// Bind Azure Service Bus settings
var azureServiceBusConfig = new ServiceBusConfig();
builder.Configuration.GetSection("AzureServiceBus").Bind(azureServiceBusConfig);
builder.Services.AddSingleton(azureServiceBusConfig);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
