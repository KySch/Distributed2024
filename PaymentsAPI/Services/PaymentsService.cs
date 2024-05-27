using MongoDB.Driver;
using PaymentsAPI.Models;
using Microsoft.Extensions.Options;
using Amazon.Runtime.Internal;

namespace PaymentsAPI.Services
{
    public class PaymentsService
    {
        private readonly IMongoCollection<Payment> _paymentsCollection;

        public PaymentsService(IOptions<DatabaseSettings> databaseSettings)
        {
            //var mongoClient = new MongoClient(
            //    databaseSettings.Value.ConnectionString);

            //var mongoDatabase = mongoClient.GetDatabase(
            //    databaseSettings.Value.DatabaseName);

            //_paymentsCollection = mongoDatabase.GetCollection<Payment>(
            //    databaseSettings.Value.PaymentsCollectionName);

            var mongoClient = new MongoClient(
                "mongodb+srv://admin:123@homeassign.vzhjtbg.mongodb.net/?retryWrites=true&w=majority&appName=HomeAssign");

            var mongoDatabase = mongoClient.GetDatabase(
                "HomeAssign");

            _paymentsCollection = mongoDatabase.GetCollection<Payment>(
                "Payments");

        }

        public async Task<List<Payment>> GetAsync() =>
            await _paymentsCollection.Find(_ => true).ToListAsync();

        public async Task<Payment?> GetAsync(string paymentId) =>
            await _paymentsCollection.Find(x => x.Id == paymentId).FirstOrDefaultAsync();   

        public async Task CreateAsync(Payment newPayment) =>
            await _paymentsCollection.InsertOneAsync(newPayment);

        public async Task RemoveAsync(string paymentId) =>
            await _paymentsCollection.DeleteOneAsync(x => x.Id == paymentId);
    }
}
