using Google.Cloud.PubSub.V1;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using PaymentsAPI.Models;

namespace PaymentsAPI.Services
{
    public class PublisherService
    {
        public TopicName topicName;
        public PublisherService(IOptions<GCPSettings> settings)
        {
            //topicName = new TopicName(settings.Value.Project, settings.Value.Topic);
            topicName = new TopicName("distributed-425112", "OrderHandling");
        }

        public async Task PublishOrderPayment(PubsubMessage message)
        {
            PublisherClient publisher = PublisherClient.Create(topicName);
            string messageId = await publisher.PublishAsync(message);
            Console.WriteLine(messageId);
            // PublisherClient instance should be shutdown after use.
            // The TimeSpan specifies for how long to attempt to publish locally queued messages.
            await publisher.ShutdownAsync(TimeSpan.FromSeconds(15));
        }
    }
}
