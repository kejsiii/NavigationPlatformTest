using Application.Utilities;
using System.IdentityModel.Tokens.Jwt;

namespace Presentation.Utilities
{
    public interface IJWTUtilities
    {
        string GenerateToken(Guid userId, string email, string role);
        JwtUser GetUserFromJWTToken(JwtSecurityToken token);
    }
}
