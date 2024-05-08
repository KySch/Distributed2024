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
        public UserController()
        {
        }

        [HttpPost("addwatchlist")]
        public async Task<ActionResult> addToWachlist()
        {
            
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] UserDTO userDTO)
        {
            var u = await _userService.GetByEmailAsync(userDTO.Email);

            if (u == null)
            {
                return BadRequest("User not found");
            }

            var isValid = u.ValidatePassword(userDTO.Password, _encryptor);

            if (!isValid)
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

            if (u == null)
            {
                return BadRequest("User not found");
            }

            var userId = _jwtBuilder.ValidateToken(token);

            if (userId != u.Id)
            {
                return BadRequest("token is not valid");
            }

            return Ok(userId);
        }



    }
}
