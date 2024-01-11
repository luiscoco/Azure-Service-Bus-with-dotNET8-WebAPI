# How to send and receive messages between .NET8 WebAPI and Azure ServiceBus

## 1. Create in Azure Portal a ServiceBus



## 2. Application configuration file (appsettings.json)

We include the appsettings.json file the: ServiceBus ConnectionString, QueueName and TopicName:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "AzureServiceBus": {
    "ConnectionString": "Endpoint=sb://mythirdservicebus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=gutLA4vD5peN8o/v9QxUh3Yy7eXH3twAg+ASbEklA0I=",
    "QueueName": "myqueue1974",
    "TopicName": "mytopic",
    "FirstSubscriptionName": "myfirstsubscription",
    "SecondSubscriptionName": "mysecondsubscription"
  }
}
```

## 3. Configure the application Middleware (Program.cs)

We bind the ServiceBus configuration in the Program.cs file:

```csharp
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
```

## 4. ServiceBus configuration Model (ServiceBusConfig.cs)

```csharp
namespace AzureServiceBus.Model
{
    public class ServiceBusConfig
    {
        public string ConnectionString { get; set; }
        public string QueueName { get; set; }
        public string TopicName { get; set; }
        public string FirstSubscriptionName { get; set; }
        public string SecondSubscriptionName { get; set; }
    }
}
```

## 5. Create the application controller (ServiceBusWebApiControllers.cs)

```csharp
using Microsoft.AspNetCore.Mvc;
using Azure.Messaging.ServiceBus;
using System.Text.Json;
using AzureServiceBus.Model;

namespace AzureServiceBus.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly ServiceBusConfig _config;

        public MessageController(ServiceBusConfig config)
        {
            _config = config;
        }

        [HttpPost("PostMessageToQueue")]
        public async Task<IActionResult> PostMessageToQueue([FromBody] object jsonData)
        {
            await using var client = new ServiceBusClient(_config.ConnectionString);
            ServiceBusSender sender = client.CreateSender(_config.QueueName);

            try
            {
                string messageBody = JsonSerializer.Serialize(jsonData);
                ServiceBusMessage message = new ServiceBusMessage(messageBody);
                await sender.SendMessageAsync(message);
                return Ok("Message sent to Azure Service Bus");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
            finally
            {
                await sender.DisposeAsync();
            }
        }

        [HttpPost("PostMessageToTopic")]
        public async Task<IActionResult> PostMessageToTopic([FromBody] object jsonData)
        {
            await using var client = new ServiceBusClient(_config.ConnectionString);
            ServiceBusSender sender = client.CreateSender(_config.TopicName);

            try
            {
                string messageBody = JsonSerializer.Serialize(jsonData);
                ServiceBusMessage message = new ServiceBusMessage(messageBody);
                await sender.SendMessageAsync(message);
                return Ok("Message sent to Azure Service Bus Topic");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
            finally
            {
                await sender.DisposeAsync();
            }
        }

        [HttpGet("ReceiveMessageFromFirstSubscription")]
        public async Task<IActionResult> ReceiveMessageFromFirstSubscription()
        {
            await using var client = new ServiceBusClient(_config.ConnectionString);
            ServiceBusReceiver receiver = client.CreateReceiver(_config.TopicName, _config.FirstSubscriptionName);

            try
            {
                ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(10));
                if (receivedMessage != null)
                {
                    string messageBody = receivedMessage.Body.ToString();
                    await receiver.CompleteMessageAsync(receivedMessage);
                    return Ok($"Received message: {messageBody}");
                }
                return NotFound("No message available in the subscription at this time.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
            finally
            {
                await receiver.DisposeAsync();
            }
        }

        [HttpGet("ReceiveMessageFromSecondSubscription")]
        public async Task<IActionResult> ReceiveMessageFromSecondSubscription()
        {
            await using var client = new ServiceBusClient(_config.ConnectionString);
            ServiceBusReceiver receiver = client.CreateReceiver(_config.TopicName, _config.SecondSubscriptionName);

            try
            {
                ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(10));
                if (receivedMessage != null)
                {
                    string messageBody = receivedMessage.Body.ToString();
                    await receiver.CompleteMessageAsync(receivedMessage);
                    return Ok($"Received message: {messageBody}");
                }
                return NotFound("No message available in the subscription at this time.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
            finally
            {
                await receiver.DisposeAsync();
            }
        }

        [HttpGet("ReceiveMessageFromQueue")]
        public async Task<IActionResult> ReceiveMessageFromQueue()
        {
            await using var client = new ServiceBusClient(_config.ConnectionString);
            ServiceBusReceiver receiver = client.CreateReceiver(_config.QueueName);

            try
            {
                ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(10));
                if (receivedMessage != null)
                {
                    string messageBody = receivedMessage.Body.ToString();
                    await receiver.CompleteMessageAsync(receivedMessage);
                    return Ok($"Received message from queue: {messageBody}");
                }
                return NotFound("No message available in the queue at this time.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
            finally
            {
                await receiver.DisposeAsync();
            }
        }

    }
}
```
