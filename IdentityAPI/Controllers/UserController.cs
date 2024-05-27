using IdentityAPI.Models;
using IdentityAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Middleware;

namespace IdentityAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly UserService _userService;
		private readonly IJwtBuilder _jwtBuilder;
		private readonly IEncryptor _encryptor;

		public UserController(UserService userService, IJwtBuilder jwtBuilder, IEncryptor encryptor)
		{
			_userService = userService;
			_jwtBuilder = jwtBuilder;
			_encryptor = encryptor;
		}

		[HttpPost("register")]
		public async Task<ActionResult> Register([FromBody] UserDTO userDTO)
		{
			var u = await _userService.GetByEmailAsync(userDTO.Email);

			if (u != null)
			{
				return BadRequest("User already exists");
			}

			var user = new User
			{
                FirstName = userDTO.FirstName,
				LastName = userDTO.LastName,
				Email = userDTO.Email
			};

			user.SetPassword(userDTO.Password, _encryptor);

			await _userService.CreateAsync(user);
			
			return Ok();
		}

		[HttpPost("login")]
		public async Task<ActionResult> Login([FromBody] UserLogin userLogin)
		{
			var u = await _userService.GetByEmailAsync(userLogin.Email);

			if(u == null)
			{
				return BadRequest("User not found");
			}

			var isValid = u.ValidatePassword(userLogin.Password, _encryptor);

			if(!isValid)
			{
				return BadRequest("Could not authenticate the user");
			}

			var token = _jwtBuilder.GetToken(u.Id);

			return Ok(token);
		}

		[HttpGet("validate")]
		public async Task<ActionResult> Validate([FromQuery(Name = "email")] string email,
			[FromQuery(Name = "token")] string token)
		{
			var u = await _userService.GetByEmailAsync(email);

			if(u == null)
			{
				return BadRequest("User not found");
			}

			var userId = _jwtBuilder.ValidateToken(token);

			if(userId != u.Id)
			{
				return BadRequest("token is not valid");
			}

			return Ok(userId);
		}

        [HttpGet("getuserdetails")]
        public async Task<ActionResult> GetUserDetails([FromQuery(Name = "userId")] string userID)
        {
            var u = await _userService.GetByIdAsync(userID);

            if (u == null)
            {
                return BadRequest("User not found");
            }

			var details = new UserDTO
			{ 
			  FirstName = u.FirstName,
              LastName = u.LastName,
              Email = u.Email,
            };
            return Ok(details);
        }

        [HttpGet("isadmin")]
        public async Task<ActionResult> IsAdmin([FromQuery(Name = "userId")] string userID)
        {
            var u = await _userService.GetByIdAsync(userID);

            if (u == null)
            {
                return BadRequest("User not found");
            }

            var details = new
            {
                Admin = u.isAdmin
            };
            return Ok(details);
        }

    }
}
