using Data.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Services.Helpers
{
    public abstract class TokenFactory
    {
        private readonly IConfiguration _config;

        public TokenFactory(IConfiguration config)
        {
            _config = config;
        }

        public JwtSecurityToken CreateJwtToken(ApplicationUser user, IList<string> roles)
        {
            try
            {
                if (user == null)
                {
                    throw new ArgumentNullException(nameof(user), "User cannot be null");
                }

                if (roles == null)
                {
                    throw new ArgumentNullException(nameof(roles), "Roles cannot be null");
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email ?? "unknow"),
                    new Claim(ClaimTypes.Name, user.UserName ?? "unknow"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                foreach (var role in roles)
                    claims.Add(new Claim(ClaimTypes.Role, role));

                var keyString = _config["JWT:KEY"];
                var issuer = _config["JWT:ISSUER"];
                var audience = _config["JWT:AUDIENCE"];

                if (string.IsNullOrEmpty(keyString))
                {
                    throw new InvalidOperationException("JWT Key is missing in configuration.");
                }

                if (keyString.Length < 32)
                {
                    throw new InvalidOperationException("JWT Key is too short.");
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var expiry = DateTime.UtcNow.AddHours(3);

                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: expiry,
                    signingCredentials: creds
                );

                return token;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create JWT token.", ex);
            }
        }

        public RefreshToken CreateRefreshToken(Guid userId, string? ip, string? userAgent)
        {
            try
            {
                var tokenString = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
                var expiry = DateTime.UtcNow.AddDays(7);

                var refreshToken = new RefreshToken
                {
                    UserId = userId,
                    Token = tokenString,
                    ExpiryDate = expiry,
                    CreatedAt = DateTime.UtcNow,
                    IpAddress = ip,
                    UserAgent = userAgent,
                    IsRevoked = false
                };

                return refreshToken;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create refresh token.", ex);
            }
        }
    }
}
