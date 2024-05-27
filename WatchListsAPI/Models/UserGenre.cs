using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Google.Cloud.PubSub.V1;

namespace WatchListsAPI.Models
{
    public class UserGenre
    {
        public string Title { get; set; }
        public string UserId { get; set; }
        public string Genre { get; set; }

        public string _id;
    }
}
