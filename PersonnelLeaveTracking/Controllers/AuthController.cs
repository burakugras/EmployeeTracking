using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PersonnelLeaveTracking.Data;
using PersonnelLeaveTracking.Helpers;
using PersonnelLeaveTracking.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PersonnelLeaveTracking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ApplicationDbContext _context;

        public AuthController(JwtSettings jwtSettings, ApplicationDbContext context)
        {
            _jwtSettings = jwtSettings;
            _context = context;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.Email == request.Email);
            if (employee == null)
            {
                return Unauthorized("E-posta adresi bulunamadı.");
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, employee.Password);
            if (!isPasswordValid)
            {
                return Unauthorized("Şifre eşleşmedi."); 
            }

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, employee.Email),
        new Claim(ClaimTypes.Role, employee.Title.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: creds
            );

            return Ok(new
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo
            });
        }


    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
