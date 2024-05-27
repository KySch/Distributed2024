using MongoDB.Driver;
using OrdersAPI.Models;
using Microsoft.Extensions.Options;
using Amazon.Runtime.Internal;

namespace OrdersAPI.Services
{
    public class OrdersService
    {
        private readonly IMongoCollection<Order> _ordersCollection;

        public OrdersService(IOptions<DatabaseSettings> databaseSettings)
        {
            //var mongoClient = new MongoClient(
            //    databaseSettings.Value.ConnectionString);

            var mongoClient = new MongoClient(
                "mongodb+srv://admin:123@homeassign.vzhjtbg.mongodb.net/?retryWrites=true&w=majority&appName=HomeAssign");



            //var mongoDatabase = mongoClient.GetDatabase(
            //    databaseSettings.Value.DatabaseName);

            var mongoDatabase = mongoClient.GetDatabase(
               "HomeAssign");

            //_ordersCollection = mongoDatabase.GetCollection<Order>(
            //    databaseSettings.Value.OrdersCollectionName);

            _ordersCollection = mongoDatabase.GetCollection<Order>(
                "Orders");
        }

        public async Task<List<Order>> GetAsync() =>
            await _ordersCollection.Find(_ => true).ToListAsync();

        public async Task<Order?> GetAsync(string userId) =>
            await _ordersCollection.Find(x => x.UserId == userId).FirstOrDefaultAsync();
        public async Task<List<Order>> GetList(string userId) =>
            await _ordersCollection.Find(x => x.UserId == userId).ToListAsync();

        public async Task CreateAsync(Order newOrder) =>
            await _ordersCollection.InsertOneAsync(newOrder);

        public async Task RemoveAsync(string orderId) =>
            await _ordersCollection.DeleteOneAsync(x => x.Id == orderId);
    }
}
