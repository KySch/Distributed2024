using Microsoft.VisualBasic;

namespace WatchListsAPI.Models
{
    public class Movie
    {
        public string Id { get; set; }

        public string? Title { get; set; }

        public string? Type { get; set; }

        public string? Genre { get; set; }
    }
}
