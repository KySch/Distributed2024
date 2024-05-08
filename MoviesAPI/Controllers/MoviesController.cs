using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class MoviesController : ControllerBase
    {
        public MoviesController()
        {

        }

        [HttpGet("byid")]
        public async Task<ActionResult> byId(string id)
        {
            var client = new RestClient("https://moviesdatabase.p.rapidapi.com/");
            var request = new RestRequest($"/titles/{id}");
            request.AddHeader("X-RapidAPI-Key", "db4add47e7msha10f25336026b5ap1fd801jsnbec5f51eebdc");
            request.AddHeader("X-RapidAPI-Host", "moviesdatabase.p.rapidapi.com");
            var response = await client.GetAsync(request); if (response.IsSuccessful)
            {
                var content = JsonConvert.DeserializeObject<JObject>(response.Content);
                string Title = content["results"]["originalTitleText"]["text"].Value<string>();
                string Type = content["results"]["titleType"]["text"].Value<string>();
                var movie = new Movie
                {
                    Id = id,
                    Title = Title,
                    Type = Type
                };
                return Ok(movie);
            }
            else
            {
                return null;
            }
        }

        [HttpGet("by_genre")]
        public async Task<ActionResult<List<Movie>>> byGenre(string genre)
        {
            var client = new RestClient("https://moviesdatabase.p.rapidapi.com/");
            var request = new RestRequest($"/titles?genre={genre}");
            request.AddHeader("X-RapidAPI-Key", "db4add47e7msha10f25336026b5ap1fd801jsnbec5f51eebdc");
            request.AddHeader("X-RapidAPI-Host", "moviesdatabase.p.rapidapi.com");

            var response = await client.GetAsync(request);
            if (response.IsSuccessful)
            {
                var content = JsonConvert.DeserializeObject<JObject>(response.Content);
                var results = content["results"].Children<JObject>();
                List<Movie> movieList = new List<Movie>();

                foreach (var item in results)
                {
                    var movie = new Movie
                    {
                        Id = item["id"].Value<string>(),
                        Title = item["originalTitleText"]["text"].Value<string>(),
                        Type = item["titleType"]["text"].Value<string>()
                    };
                    movieList.Add(movie); // Ensure this line is within the loop
                }

                return Ok(movieList);
            }
            else
            {
                return StatusCode((int)response.StatusCode); // Provide the HTTP status code from the failed response
            }
        }
    }
}

