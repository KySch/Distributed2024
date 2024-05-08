using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FootballAPI.Models;
using MoviesAPI.Models;

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class MoviesController : ControllerBase
    {
        private readonly MovieContext _movieContext;
        public MoviesController(MovieContext movieContext)
        {
            _movieContext = movieContext;
        }

        [HttpGet("byid")]
        public async Task<ActionResult> byId(long id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _movieContext.RetriveMovieFromId(id);
            if (movie == null)
            {
                return NotFound();
            }

            return Ok();
        }
    }
}
