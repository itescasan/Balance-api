
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
using Microsoft.AspNetCore.Http;
using System.Net;
namespace Balance_api.Services
{
    public class ValidarTokenHandler : DelegatingHandler
    {

        //protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        //{
        //    HttpStatusCode statusCode;
        //    string token;
        //    string json = string.Empty;

        //    if (!TryRetrieveToken(request, out token))
        //    {
        //        statusCode = HttpStatusCode.Unauthorized;

        //        return base.SendAsync(request, cancellationToken);
        //    }

        //    try
        //    {
        //       // var claveSecreta = ConfigurationManager.AppSettings["ClaveSecreta"];
        //       // var issuerToken = ConfigurationManager.AppSettings["Issuer"];
        //        //var audienceToken = ConfigurationManager.AppSettings["Audience"];

        //        //var securityKey = new SymmetricSecurityKey(
        //            //System.Text.Encoding.Default.GetBytes(claveSecreta));

        //        //SecurityToken securityToken;
        //        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        //        TokenValidationParameters validationParameters = new TokenValidationParameters()
        //        {
        //            //ValidAudience = audienceToken,
        //           //ValidIssuer = issuerToken,
        //            ValidateLifetime = true,
        //            ValidateIssuerSigningKey = true,
        //            // DELEGADO PERSONALIZADO PERA COMPROBAR
        //            // LA CADUCIDAD EL TOKEN.
        //            //LifetimeValidator = this.LifetimeValidator,
        //            //IssuerSigningKey = securityKey
        //        };


        //        //AutorizeController cont = new AutorizeController();
        //        //string RefresToken = cont.RefrescarToken(token);

        //        //if (RefresToken != string.Empty) token = RefresToken;



        //        // COMPRUEBA LA VALIDEZ DEL TOKEN
        //        //Thread.CurrentPrincipal = tokenHandler.ValidateToken(token,
        //                                                             //validationParameters,
        //                                                             //out securityToken);
        //      //  HttpContext.Current.User = tokenHandler.ValidateToken(token,
        //                                                              //validationParameters,
        //                                                             // out securityToken);

        //        return base.SendAsync(request, cancellationToken);
        //    }
        //    catch (SecurityTokenValidationException ex)
        //    {

        //        statusCode = HttpStatusCode.Unauthorized;


        //    }
        //    catch (Exception ex)
        //    {
        //        statusCode = HttpStatusCode.InternalServerError;
        //    }



        //    return Task<HttpResponseMessage>.Factory.StartNew(() =>
        //               new HttpResponseMessage(statusCode) { });
        //}



        // RECUPERA EL TOKEN DE LA PETICIÓN
        //private static bool TryRetrieveToken(HttpRequestMessage request, out string token)
        //{
        //    token = null;
        //    IEnumerable<string> authzHeaders;
        //    if (!request.Headers.TryGetValues("Authorization", out authzHeaders) ||
        //                                      authzHeaders.Count() > 1)
        //    {
        //        return false;
        //    }
        //    var bearerToken = authzHeaders.ElementAt(0);
        //    token = bearerToken.StartsWith("Bearer ") ?
        //            bearerToken.Substring(7) : bearerToken;

        //    //token = token.Replace("\"", "");
        //    return true;
        //}

    }
}
