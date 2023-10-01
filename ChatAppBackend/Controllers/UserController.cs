using Api.DataAccess.Data;
using Api.DataAccess.Helper;
using Api.Models;
using Azure.Messaging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace ChatAppBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] User userObj)
        {
            if (userObj == null)
                return BadRequest();
            var user = await _context.users
                .FirstOrDefaultAsync(x=> x.Email == userObj.Email);

            if(user == null)
                return NotFound(new {Message = "User Not Found!"});
            
            if (!PasswordHasher.VerifyPassword(userObj.Password, user.Password))
                return BadRequest(new { Message = "Password Incorrect!" });

            user.Token = CreateJwt(user);

            return Ok(new
            {
                Token = user.Token,
                Message = "Login Success!"
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] User userObj)
        {
            if(userObj == null) 
                return BadRequest();
            //Check Email
            if (await CheckEmailExistsAsync(userObj.Email))
                return BadRequest(new {Message="Email Already in Use!"});
            //Check Password Strength
            var pass = CheckPasswordStrength(userObj.Password);
            if (!string.IsNullOrEmpty(pass))
                return BadRequest(new { Message = pass.ToString()});
            userObj.Password = PasswordHasher.EncodePassword(userObj.Password);
            userObj.Token = "";
            await _context.users.AddAsync(userObj);
            await _context.SaveChangesAsync();
            return Ok(new {
                Message = "User Registered!"
            });
        }

        private Task<bool> CheckEmailExistsAsync(string Email)
            =>  _context.users.AnyAsync(x => x.Email == Email);

        private string CheckPasswordStrength(string password)
        {
            StringBuilder sb = new StringBuilder();
            if(password.Length < 8)
                sb.Append("Minimum Password length should be 8"+ Environment.NewLine);
            if (!(Regex.IsMatch(password, "[a-z]") && Regex.IsMatch(password, "[A-Z]") && Regex.IsMatch(password, "[0-9]")))
                sb.Append("Password should contain a-z or A-Z and 0-9" + Environment.NewLine);
            if (!(Regex.IsMatch(password, "[<,>,@,!,#,$,%,^,&,*,(,),_,=,+,|]")))
                sb.Append("Password should contain special characters");
            return sb.ToString();
        }

        private string CreateJwt(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("ChatAppBackendApplication");
            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Name , user.Name),
                new Claim(ClaimTypes.Email, user.Email)
            });

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);

            return jwtTokenHandler.WriteToken(token);
        }
        [HttpGet]
        public async Task<ActionResult<User>> GetAllUsers()
        {
            return Ok(await _context.users.ToListAsync());
        }
    }
}
