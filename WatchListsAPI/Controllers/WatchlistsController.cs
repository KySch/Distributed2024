using WatchListsAPI.Models;
using WatchListsAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Middleware;

namespace PaymentsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WatchlistsController : ControllerBase
    {
        private readonly WatchlistsService _watchlistsService;
        private readonly IJwtBuilder _jwtBuilder;
        private readonly IEncryptor _encryptor;

        public WatchlistsController(WatchlistsService watchlistsService, IJwtBuilder jwtBuilder, IEncryptor encryptor)
        {
            _watchlistsService = watchlistsService;
            _jwtBuilder = jwtBuilder;
            _encryptor = encryptor;
        }

        [HttpPost("newWatchlist")]
        public async Task<ActionResult> NewWatchList([FromBody] Watchlist watchlists)
        {

            var watchlist = new Watchlist
            {
                UserId = watchlists.UserId,
                MovieId = watchlists.MovieId
            };

            await _watchlistsService.CreateAsync(watchlist);

            return Ok();
        }
        [HttpPost("getWatchlist")]
        public async Task<ActionResult> GetWatchlistAsync([FromBody] Watchlist userId)
        {
            var watchlist = await _watchlistsService.GetAsync(userId.UserId);

            if (watchlist == null)
            {
                return BadRequest("Watchlist not found");
            }

            return Ok(watchlist);
        }


        [HttpPost("removeWatchlist")]
        public async Task<ActionResult> RemoveFromWatchlist([FromBody] Watchlist watchlist)
        {
            try
            {
                var newwatchlist = await _watchlistsService.GetAsync(watchlist.UserId, watchlist.MovieId);

                await _watchlistsService.RemoveAsync(watchlist.Id);

                if (watchlist == null)
                {
                    return BadRequest("Watchlist not found");
                }
                return Ok(watchlist);
            }
            catch (Exception ex)
            {

            }

            return Ok();
        }
    }
}
