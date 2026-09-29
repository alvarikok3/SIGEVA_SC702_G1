using Microsoft.IdentityModel.Tokens;
using SIGEVA_API.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SIGEVA_API.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerarToken(Usuario usuario)
        {
            var claims = new[]
            {
                new Claim("IdUsuario", usuario.IdUsuario.ToString()),
                new Claim("IdRol", usuario.IdRol.ToString()),
                new Claim("Nombre", usuario.Nombre),
                new Claim("Correo", usuario.Correo)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
            );

            var credenciales = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: credenciales
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}