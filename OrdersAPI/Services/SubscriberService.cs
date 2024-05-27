
using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Options;
using MongoDB.Bson.IO;
using OrdersAPI.Models;
using OrdersAPI.Services;
using Newtonsoft.Json;
using Google.Protobuf;
using RestSharp;
using Newtonsoft.Json.Linq;

namespace OrdersAPI.Services
{
    public class SubscriberService : IHostedService, IDisposable
    {
        private readonly OrdersService _ordersService;
        private readonly SubscriptionName subscriptionName;
        private readonly SubscriptionName NotificationSubscriptionName;
        private SubscriberClient subscriber;
        private SubscriberClient notifSub;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly PublisherService _publisherService;

        public SubscriberService(IOptions<GCPSettings> settings,
                                OrdersService OrdersService,
                                PublisherService publisherService)
        {
            subscriptionName = new SubscriptionName(settings.Value.Project, "ProcessMovie"); //"upcomingPubSub" project
            NotificationSubscriptionName = new SubscriptionName(settings.Value.Project, "UpcomingOrders");
            _ordersService = OrdersService;
            _publisherService = publisherService;
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

            var order = new Order
            {
                UserId = json.UserId,
                MovieId = json.MovieId
            };

            await _ordersService.CreateAsync(order);
            try
            {
                var client = new RestClient("http://localhost:5032/api/watchlists/");
                var request = new RestRequest("removeWatchlist", Method.Post);
                var body = new Watchlist
                {
                    UserId = order.UserId,
                    MovieId = order.MovieId
                };
                request.AddBody(body);
                var response = await client.PostAsync(request);


                Console.WriteLine("RemoveWatchlist: " + response.StatusCode);
            }
            catch
            {

            }
            

            return SubscriberClient.Reply.Ack;
        }

        private async Task<SubscriberClient.Reply> HandleUpcomingNotification(PubsubMessage msg, CancellationToken cancellationToken)
        {
            List<UserGenre> userGenre = new List<UserGenre>();
            Console.WriteLine($"Received message {msg.MessageId} published at {msg.PublishTime.ToDateTime()}");
            Console.WriteLine($"Text: '{msg.Data.ToStringUtf8()}'");

            // Decode the base64 data string
            string dataString = msg.Data.ToStringUtf8();
            Console.WriteLine($"Data: '{dataString}'");

            // Deserialize the attributes
            var attributesJson = Newtonsoft.Json.JsonConvert.SerializeObject(msg.Attributes);
            var attributes = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(attributesJson);

            // Access properties

            string movieId = attributes.UpcomingMovie;
            var client = new RestClient("http://localhost:5174/api/Movies/");
            var request = new RestRequest("/byId", Method.Post);
            var body = new Movie
            {
                Id = movieId,
            };
            request.AddBody(body);
            var response = await client.PostAsync(request);
            var content = JObject.Parse(response.Content);
            string genre = content["genre"].Value<string>();



            List<Order> watchArray = _ordersService.GetAsync().Result;

            foreach (var item in watchArray)
            {
                Order order = await _ordersService.GetAsync(item.MovieId.ToString());


                var newresponse = await client.PostAsync(request);
                var newcontent = JObject.Parse(response.Content);
                string Id = content["id"].Value<string>();
                string Title = content["title"].Value<string>();
                string Type = content["type"].Value<string>();
                string Genre = content["genre"].Value<string>();
                var movie = new Movie
                {
                    Id = movieId,
                    Title = Title,
                    Type = Type,
                    Genre = Genre
                };
                UserGenre newGenre = new UserGenre { _id = item.Id, UserId = item.UserId, Genre = movie.Genre, Title = movie.Title };
                userGenre.Add(newGenre);
            }

            var uniqueUserGenres = userGenre
                .GroupBy(ug => new { ug.UserId, ug.Genre })
                .Select(g => g.First())
                .ToList();

            foreach (var item in uniqueUserGenres)
            {
                if (string.Equals(genre, item.Genre, StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine("Item With Same Genre Found");
                    // Now take userId and push notification about the new movie
                    var notification = new PubsubMessage
                    {
                        Data = ByteString.CopyFromUtf8(item.UserId),
                        Attributes =
                                {
                                    { "UserId", item.UserId },
                                    { "Movie", item.Title }
                                }
                    };
                    System.Diagnostics.Debug.WriteLine("Saved");
                    await _publisherService.PublishNotification(notification);
                }
            }

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