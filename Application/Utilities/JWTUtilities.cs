using Application.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Presentation.Utilities
{
    public class JWTUtilities : IJWTUtilities
    {
        private readonly IConfiguration _configuration;
        public JWTUtilities(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string GenerateToken(Guid userId, string email, string role)
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role)
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public JwtUser GetUserFromJWTToken(JwtSecurityToken token)
        {
            var claims = token.Claims;
            string role = null;

            if (claims.FirstOrDefault(c => c.Type == ClaimTypes.Role) != null)
            {
                role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value;
            }
            else if (claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid) != null)
            {
                role = $"Someone on {claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value}, " +
                $"{claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value}, " +
                $"{claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value}" +
                $"{claims.FirstOrDefault(c => c.Type == "Password")?.Value}"
                ;
            }
            else
            {
                role = "None";
            }
            string roleName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            string id = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;  
            string email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;             
            string password = claims.FirstOrDefault(c => c.Type == "Password")?.Value;

            return new JwtUser()
            {
                Role = roleName,
                Id = id,
                Email = email,
                Password = password
            };
        }
    }
}
