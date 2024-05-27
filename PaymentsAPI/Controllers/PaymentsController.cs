using PaymentsAPI.Models;
using PaymentsAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Middleware;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;

namespace PaymentsAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PaymentsController : ControllerBase
	{
		public PaymentsService _paymentsService;
        public PublisherService _publisher;
        private readonly IJwtBuilder _jwtBuilder;
		private readonly IEncryptor _encryptor;
        

		public PaymentsController(PaymentsService paymentsService, PublisherService publisherService, IJwtBuilder jwtBuilder, IEncryptor encryptor)
		{
            _paymentsService = paymentsService;
			_jwtBuilder = jwtBuilder;
			_encryptor = encryptor;
            _publisher = publisherService;

        }

        [HttpPost("newPayment")]
        public async Task<ActionResult> NewPayment([FromBody] Payment payments)
        {
            string paymentId = payments.Id ?? Guid.NewGuid().ToString();
            var payment = new PubsubMessage
            {
                Data = ByteString.CopyFromUtf8(paymentId),
                Attributes =
                {
                    { "UserId", payments.UserId },
                    { "MovieId",  payments.MovieId }
                }
                
            };

            await _paymentsService.CreateAsync(payments);

            await _publisher.PublishOrderPayment(payment);
            return Ok();
        }

        [HttpGet("getPayment")]
        public async Task<ActionResult> GetPaymentAsync()
        {
            var payment = await _paymentsService.GetAsync();

            if (payment == null)
            {
                return BadRequest("Payment not found");
            }

            return Ok(payment);
        }
    }
}
