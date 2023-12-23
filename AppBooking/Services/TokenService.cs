using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using AppBooking.Model;
using Microsoft.EntityFrameworkCore;
using AppBooking.Data;

namespace AppBooking.Services
{
    public class TokenService(IConfiguration configuration,AppDbContext appDbContext)
    {
        private readonly string _secretKey = configuration["Jwt:Key"];
        private readonly string _issuer = configuration["Jwt:Issuer"];
        private readonly AppDbContext _context = appDbContext;
        public string GenerateToken(User user)
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { 
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        public ClaimsPrincipal VerifyToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _issuer,
                ValidAudience = _issuer,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
                return principal;
            }
            catch (Exception)
            {
                // Token validation failed
                return null;
            }
        }
        public string GetUserIdFromToken(string token)
        {
            var claimsPrincipal = VerifyToken(token);
            Console.WriteLine(claimsPrincipal.Claims);
            var userIdClaim = claimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier);

            return userIdClaim?.Value;
        }

        public string GetUserRoleFromToken(string token)
        {
            var claimsPrincipal = VerifyToken(token);
            var roleClaim = claimsPrincipal?.FindFirst(ClaimTypes.Role);

            return roleClaim?.Value;
        }
        public async Task<User> GetUserAsync(string token)
        {
            int userId = int.Parse(this.GetUserIdFromToken(token));
            User? user = await _context.Users.FirstOrDefaultAsync(m => m.UserId == userId);
            return user;

        }
    }
}
