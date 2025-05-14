using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using System.Collections.Generic;

namespace Adventure19.Services
{
    public class JwtService
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly int _expiresInMinutes;

        public JwtService(IConfiguration configuration)
        {
            _secretKey = configuration["Jwt:Key"];
            _issuer = configuration["Jwt:Issuer"];
            _expiresInMinutes = int.Parse(configuration["Jwt:ExpiresInMinutes"]);
        }

        public string GenerateToken(string userId, string userEmail, string useCase = "auth") // UseCase è un claim personalizzato
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, userEmail),
                new Claim(JwtRegisteredClaimNames.Sub, userEmail),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("use",useCase) // Token che può essere usato per più scopi
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _issuer,  // In assenza di un pubblico specifico, puoi usare lo stesso dell'issuer
                claims: claims,
                expires: DateTime.Now.AddMinutes(_expiresInMinutes),  // Usa la durata di scadenza configurata
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal ValidateToken(string token)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    IssuerSigningKey = key,
                    ValidIssuer = _issuer,
                    ValidAudience = _issuer  // Usa lo stesso dell'issuer come valid audience
                }, out var validatedToken);

                return principal;
            }
            catch (Exception)
            {
                return null; // Token non valido
            }
        }
    }
}
