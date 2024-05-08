using Microsoft.EntityFrameworkCore;
using MoviesAPI.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Collections.Generic;
using System.Numerics;
namespace FootballAPI.Models
{
    public class MovieContext : DbContext
    {
        public MovieContext(DbContextOptions<MovieContext> options) : base(options) { }

        public DbSet<Movie> Movies { get; set; } = null!;

        public async Task<Movie?> RetriveMovieFromId(long id)
        {
            var client = new RestClient("https://moviesdatabase.p.rapidapi.com/"); 
            var request = new RestRequest($"/titles/{id}"); 
            request.AddHeader("X-RapidAPI-Key", "db4add47e7msha10f25336026b5ap1fd801jsnbec5f51eebdc");
            request.AddHeader("X-RapidAPI-Host", "moviesdatabase.p.rapidapi.com");
            var response = await client.GetAsync(request); if (response.IsSuccessful)
            {
                var content = JsonConvert.DeserializeObject<JToken>(response.Content);
                string Title = content["results"]["originalTitleText"]["text"].Value<string>();
                string Type = content["results"]["titleType"]["text"].Value<string>();
                DateTime ReleaseDate = content["results"]["releaseDate"].Value<DateTime>();
                return new Movie
                {
                    Id = id,
                    Title = Title,
                    Type = Type,
                    ReleaseDate = ReleaseDate
                };
            }
            else
            {
                return null;
            }
        }
    }

}


