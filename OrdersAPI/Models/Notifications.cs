using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace OrdersAPI.Models
{
    public class Notifications
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string? description { get; set; }
    }
}
