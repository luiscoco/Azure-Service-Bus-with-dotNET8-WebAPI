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
