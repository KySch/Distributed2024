using Microsoft.VisualBasic;

namespace MoviesAPI.Models
{
    public class Movie
    {
        public long Id { get; set; }

        public string? Title { get; set; }

        public string? Type { get; set; }

        public DateTime? ReleaseDate { get; set; }
    }
}
