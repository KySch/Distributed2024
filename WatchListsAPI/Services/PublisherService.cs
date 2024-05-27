using Google.Cloud.PubSub.V1;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using WatchListsAPI.Models;

namespace WatchListsAPI.Services
{
    public class PublisherService
    {
        public TopicName topicName;
        public PublisherService(IOptions<GCPSettings> settings)
        {
            topicName = new TopicName("distributed-425112", "SendNotification");
        }

        public async Task PublishNotification(PubsubMessage message)
        {
            PublisherClient publisher = PublisherClient.Create(topicName);
            string messageId = await publisher.PublishAsync(message);
            Console.WriteLine(messageId);

            await publisher.ShutdownAsync(TimeSpan.FromSeconds(15));
        }
    }
}
