using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PaymentsAPI.Models
{
	public class Payment
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string? Id { get; set; }
        public string UserId { get; set; }
        public string MovieId { get; set; }

    }
}
