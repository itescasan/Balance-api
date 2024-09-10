using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Balance_api.Models;
using Balance_api.Models.Custom;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Balance_api.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Balance_api.Services
{
    public class AutorizacionService : IAutorizacionService
    {
        private readonly BalanceEntities _balanceEntities;
        private readonly IConfiguration _configuration;

        public AutorizacionService(BalanceEntities balanceEntities, IConfiguration configuration)
        {
            _balanceEntities = balanceEntities;
            _configuration = configuration;
        }

        private string GenerarToken(string IdUsuario)
        {

            var key = _configuration.GetValue<string>("Jwt:Key");
            var KeyBytes = Encoding.ASCII.GetBytes(key);

            var claims = new ClaimsIdentity();
            claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, IdUsuario));

            var credencialesToken = new SigningCredentials(
                new SymmetricSecurityKey(KeyBytes),
                SecurityAlgorithms.HmacSha256Signature
                );

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddMinutes(1),
                SigningCredentials = credencialesToken
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenConfig = tokenHandler.CreateToken(tokenDescriptor);

            string tokenCreado = tokenHandler.WriteToken(tokenConfig);

            return tokenCreado;
        }

        private string GenerateRefreshToken()
        {
            var byteArray = new byte[64];
            var refreshToken = "";

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(byteArray);
                refreshToken = Convert.ToBase64String(byteArray);
            }
            return refreshToken;
        }

        private async Task<AutorizacionResponse> GuardarHistorialRefreshToken(
            int IdUsuario,
            string token,
            string refreshToken
            ){

            var historialRefreshToken = new HistorailRefreshToken
            {
                IdUsuario = IdUsuario,
                Token = token,
                RefreshToken = refreshToken,
                FechaCreacion = DateTime.UtcNow,
                FechaExpiracion = DateTime.UtcNow.AddMinutes(2)
            };

            await _balanceEntities.HistorailRefreshToken.AddAsync(historialRefreshToken);
            await _balanceEntities.SaveChangesAsync();  

            return new AutorizacionResponse { Token = token,RefreshToken = refreshToken,Resultado = true, Msg = "Ok" };
        }         
            

        public async Task<AutorizacionResponse> DevolverToken(AutorizacionRequest autorizacion)
        {
            var usuario_encontrado = _balanceEntities.Usuarios.FirstOrDefault(x => 
            x.Usuario == autorizacion.NombreUsuario);

            string sQuery = $"SELECT [SIS].[Desencriptar](  {"0x"}{BitConverter.ToString(usuario_encontrado!.Pass).Replace("-", "")}) AS Pass";
            string Pwd = _balanceEntities.Database.SqlQueryRaw<string>(sQuery).ToList().First();

            if (usuario_encontrado == null && Pwd == null)
            {
                return await Task.FromResult<AutorizacionResponse>(null);
            }

            string tokenCreado = GenerarToken(usuario_encontrado.IdUsuario.ToString());

            string refreshTokenCreado = GenerateRefreshToken();

            //return new AutorizacionResponse() { Token = tokenCreado, Resultado = true,Msg = "Ok" };

            return await GuardarHistorialRefreshToken(usuario_encontrado.IdUsuario,tokenCreado,refreshTokenCreado);
        }

        public async Task<AutorizacionResponse> DevolverRefreshToken(RefreshTokenRequest refreshTokenRequest, int IdUsurio)
        {
            var refreshTokenEncontrado = _balanceEntities.HistorailRefreshToken.FirstOrDefault(x =>
            x.Token == refreshTokenRequest.TokenExpirado &&
            x.RefreshToken == refreshTokenRequest.RefreshToken &&
            x.IdUsuario == IdUsurio);

            if (refreshTokenEncontrado == null)
                return new AutorizacionResponse { Resultado = false, Msg = "No Existe Refresh Token" };

            var refreshTokenCreado = GenerateRefreshToken();
            var tokenCreado = GenerarToken(IdUsurio.ToString());

            return await GuardarHistorialRefreshToken(IdUsurio,tokenCreado, refreshTokenCreado);
        }
    }
}
