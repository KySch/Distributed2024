
using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Options;
using MongoDB.Bson.IO;
using WatchListsAPI.Models;
using WatchListsAPI.Services;
using Newtonsoft.Json;
using RestSharp;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;
using static Google.Apis.Requests.BatchRequest;
using Google.Protobuf;
using Microsoft.VisualBasic;



namespace WatchListsAPI.Services
{
    public class SubscriberService : IHostedService, IDisposable
    {
        private Timer? _timer = null;
        private readonly WatchlistsService _watchlistsService;
        private readonly SubscriptionName subscriptionName;
        private readonly PublisherService _publisherService;

        Movie movie = new Movie();
        public SubscriberService(IOptions<GCPSettings> settings,
                                WatchlistsService watchlistsService,
                                PublisherService publisherService
                                )
        {
            subscriptionName = new SubscriptionName(settings.Value.Project, "UpcomingWatchlist");
            _watchlistsService = watchlistsService;
            _publisherService = publisherService;
            
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("SubscriberService running.");

            _timer = new Timer(async _ => await DoWork(_), null, TimeSpan.Zero,
                TimeSpan.FromSeconds(10));

            return Task.CompletedTask;
        }

        private volatile bool _isWorking = false;

        private async Task DoWork(object? state)
        {
            List<UserGenre> userGenre = new List<UserGenre>();
            if (_isWorking) return;
            _isWorking = true;

            try
            {
                // Gets the message, deserializes and access its properties
                SubscriberClient subscriber = await SubscriberClient.CreateAsync(subscriptionName);
                List<PubsubMessage> receivedMessages = new List<PubsubMessage>();

                await subscriber.StartAsync(async (msg, cancellationToken) =>
                {
                    receivedMessages.Add(msg);
                    Console.WriteLine($"Received message {msg.MessageId} published at {msg.PublishTime.ToDateTime()}");
                    Console.WriteLine($"Text: '{msg.Data.ToStringUtf8()}'");

                    string dataString = msg.Data.ToStringUtf8();
                    Console.WriteLine($"Data: '{dataString}'");

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

                    List<Watchlist> watchArray = _watchlistsService.GetAsync().Result;

                    foreach (var item in watchArray)
                    {
                        Movie movie = await getGenre(item.MovieId.ToString());   
                        UserGenre newGenre = new UserGenre {_id = item.Id, UserId = item.UserId, Genre = movie.Genre, Title = movie.Title };
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

                    subscriber.StopAsync(TimeSpan.FromSeconds(10));
                    return SubscriberClient.Reply.Ack;
                });
            }
            finally
            {
                _isWorking = false;
            }
        }



        public Task StopAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("SubscriberService is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public async Task<Movie> getGenre(string movieId)
        {
            using (var client = new RestClient("http://localhost:5174/api/Movies/"))
            {
                var request = new RestRequest("/byId", Method.Post);
                var body = new Movie
                {
                    Id = movieId,
                };
                request.AddBody(body);
                var response = await client.PostAsync(request);

                if (response.IsSuccessful && response.Content != null)
                {
                    try
                    {
                        var content = JObject.Parse(response.Content);
                        if (content.ContainsKey("genre") && content["genre"].Type != JTokenType.Null)
                        {
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
                            return movie;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Genre not found or is null in the response.");
                        }
                    }
                    catch (JsonException ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Error parsing JSON response: " + ex.Message);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Failed to get a successful response or the response was empty.");
                }

                return movie;
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
