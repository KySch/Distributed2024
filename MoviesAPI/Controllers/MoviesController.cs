using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MoviesAPI.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace MoviesAPI.Controllers
{
    public class MovieIdRequest
    {
        public string Id { get; set; }
    }

    public class GenreRequest
    {
        public string Genre { get; set; }
    }

    public class TypeGenreRequest
    {
        public string Type { get; set; }
        public string Genre { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        public MoviesController()
        {
        }

        [HttpPost("byId")]
        public async Task<ActionResult> byId([FromBody] Movie request)
        {
            var client = new RestClient("https://moviesdatabase.p.rapidapi.com/");
            var restRequest = new RestRequest($"/titles/{request.Id}?info=base_info");

            restRequest.AddHeader("X-RapidAPI-Key", "db4add47e7msha10f25336026b5ap1fd801jsnbec5f51eebdc");
            restRequest.AddHeader("X-RapidAPI-Host", "moviesdatabase.p.rapidapi.com");
            var response = await client.GetAsync(restRequest);
            if (response.IsSuccessful)
            {
                var content = JsonConvert.DeserializeObject<JObject>(response.Content);
                string Title = content["results"]["titleText"]["text"].Value<string>();
                string Type = content["results"]["titleType"]["text"].Value<string>();
                string Genre = content["results"]["genres"]["genres"][0]["text"].Value<string>();
                var movie = new Movie
                {
                    Id = request.Id,
                    Title = Title,
                    Type = Type,
                    Genre = Genre
                };
                return Ok(movie);
            }
            else
            {
                return StatusCode((int)response.StatusCode);
            }
        }

        [HttpPost("byGenre")]
        public async Task<ActionResult<List<Movie>>> byGenre([FromBody] GenreRequest request)
        {
            var client = new RestClient("https://moviesdatabase.p.rapidapi.com/");
            var restRequest = new RestRequest($"/titles?genre={request.Genre}");
            restRequest.AddHeader("X-RapidAPI-Key", "db4add47e7msha10f25336026b5ap1fd801jsnbec5f51eebdc");
            restRequest.AddHeader("X-RapidAPI-Host", "moviesdatabase.p.rapidapi.com");

            var response = await client.GetAsync(restRequest);
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
                    movieList.Add(movie);
                }

                return Ok(movieList);
            }
            else
            {
                return StatusCode((int)response.StatusCode);
            }
        }

        [HttpPost("search")]
        public async Task<ActionResult> getbyTypeGenre([FromBody] TypeGenreRequest request)
        {
            var client = new RestClient("https://moviesdatabase.p.rapidapi.com/");
            var restRequest = new RestRequest($"/titles?genre={request.Genre}&titleType={request.Type}");
            restRequest.AddHeader("X-RapidAPI-Key", "db4add47e7msha10f25336026b5ap1fd801jsnbec5f51eebdc");
            restRequest.AddHeader("X-RapidAPI-Host", "moviesdatabase.p.rapidapi.com");

            var response = await client.GetAsync(restRequest);
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
                    movieList.Add(movie);
                }

                return Ok(movieList);
            }
            else
            {
                return StatusCode((int)response.StatusCode);
            }
        }
    }
}

