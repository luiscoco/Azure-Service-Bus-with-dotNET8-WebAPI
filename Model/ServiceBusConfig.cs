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
