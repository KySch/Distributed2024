using IdentityAPI.Models;
using IdentityAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Middleware;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;

namespace IdentityAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UpcomingController : ControllerBase
    {
        public PublisherService _publisher;
        private readonly IJwtBuilder _jwtBuilder;
        private readonly IEncryptor _encryptor;


        public UpcomingController(PublisherService publisherService, IJwtBuilder jwtBuilder, IEncryptor encryptor)
        {
            _jwtBuilder = jwtBuilder;
            _encryptor = encryptor;
            _publisher = publisherService;
        }

        [HttpPost("newUpcoming")]
        public async Task<ActionResult> NewPayment([FromBody] Upcoming upcoming)
        {

            var upcomingMovie = new PubsubMessage
            {
                Data = ByteString.CopyFromUtf8(upcoming.Id),
                Attributes =
                {
                    { "UpcomingMovie", upcoming.UpcomingMovie }
                }

            };

            // await _paymentsService.CreateAsync(payments);

            await _publisher.PublishOrderPayment(upcomingMovie);
            return Ok(upcomingMovie);
        }
    }
}