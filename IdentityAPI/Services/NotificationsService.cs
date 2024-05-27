using MongoDB.Driver;
using IdentityAPI.Models;
using Microsoft.Extensions.Options;
using Amazon.Runtime.Internal;

namespace IdentityAPI.Services
{
    public class NotificationsService
    {
        private readonly IMongoCollection<Notifications> _notificationsCollection;

        public NotificationsService(IOptions<DatabaseSettings> databaseSettings)
        {
            var mongoClient = new MongoClient(
                databaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                databaseSettings.Value.DatabaseName);

            _notificationsCollection = mongoDatabase.GetCollection<Notifications>(
                databaseSettings.Value.NotificationsCollectionName);
        }

        public async Task<List<Notifications>> GetAsync() =>
            await _notificationsCollection.Find(_ => true).ToListAsync();

        public async Task<List<Notifications>> GetAsync(string userId) =>
            await _notificationsCollection.Find(x => x.UserId == userId).ToListAsync();   

        public async Task CreateAsync(Notifications newNotification) =>
            await _notificationsCollection.InsertOneAsync(newNotification);

        public async Task RemoveAsync(string userId) =>
            await _notificationsCollection.DeleteOneAsync(x => x.UserId == userId);
    }
}
