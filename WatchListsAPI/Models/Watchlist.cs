using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WatchListsAPI.Models
{
	public class Watchlist
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string? Id { get; set; }
        public string UserId { get; set; }
        public string? MovieId { get; set; }

	}
}
