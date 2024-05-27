using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Options;
using MongoDB.Bson.IO;
using IdentityAPI.Models;
using IdentityAPI.Services;
using Newtonsoft.Json;
using System.Threading.Channels;

namespace IdentityAPI.Services
{
    public class SubscriberService : IHostedService, IDisposable
    {
        private readonly NotificationsService _notificationsService;
        private readonly SubscriptionName subscriptionName;
        private readonly SubscriptionName NotificationSubscriptionName;
        private SubscriberClient subscriber;
        private SubscriberClient notifSub;
        private CancellationTokenSource _cancellationTokenSource;

        public SubscriberService(IOptions<GCPSettings> settings,
                                NotificationsService notificationsService)
        {
            subscriptionName = new SubscriptionName(settings.Value.Project, "OrderNotification"); //"upcomingPubSub" project
            NotificationSubscriptionName = new SubscriptionName(settings.Value.Project, "ProcessNotification");
            _notificationsService = notificationsService;
        }

        public async Task StartAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("SubscriberService running.");
            subscriber = await SubscriberClient.CreateAsync(subscriptionName);
            notifSub = await SubscriberClient.CreateAsync(NotificationSubscriptionName);
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

            Task.Run(() => ProcessMessages(subscriber, HandleOrderConfirmation, _cancellationTokenSource.Token));
            Task.Run(() => ProcessMessages(notifSub, HandleUpcomingNotification, _cancellationTokenSource.Token));
        }

        private async Task ProcessMessages(SubscriberClient client, Func<PubsubMessage, CancellationToken, Task<SubscriberClient.Reply>> handler, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await client.StartAsync(handler);
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }

        private async Task<SubscriberClient.Reply> HandleOrderConfirmation(PubsubMessage msg, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Received message {msg.MessageId} published at {msg.PublishTime.ToDateTime()}");
            Console.WriteLine($"Text: '{msg.Data.ToStringUtf8()}'");

            string JsonData = msg.Attributes.ToString();
            var json = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(JsonData);

            var notification = new Notifications
            {
                Id = json.Id,
                UserId = json.UserId,
                description = ("Movie purchase: " + json.MovieId)
            };

            await _notificationsService.CreateAsync(notification);

            return SubscriberClient.Reply.Ack;
        }

        private async Task<SubscriberClient.Reply> HandleUpcomingNotification(PubsubMessage msg, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Received message {msg.MessageId} published at {msg.PublishTime.ToDateTime()}");
            Console.WriteLine($"Text: '{msg.Data.ToStringUtf8()}'");

            string JsonData = msg.Attributes.ToString();
            var json = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(JsonData);

            var notification = new Notifications
            {
                UserId = json.UserId,
                description = ("Upcoming movie " + json.Movie)
            };

            await _notificationsService.CreateAsync(notification);

            return SubscriberClient.Reply.Ack;
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("SubscriberService is stopping.");
            _cancellationTokenSource.Cancel();

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
        }
    }
}
