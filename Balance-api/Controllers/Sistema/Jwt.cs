using Balance_api.Contexts;
using Balance_api.Models.Sistema;
using System.Security.Claims;



namespace Balance_api.Controllers.Sistema
{
    public class Jwt
    {
       
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Subject {  get; set; }
        public static dynamic validarToken(ClaimsIdentity identity)
        {
            try
            {
                if (identity.Claims.Count() == 0)
                {
                    return new
                    {
                        success = false,
                        message = "Verificar Si estas Enviando un Token Valido",
                        Results = ""
                    };
                }
                var pass = identity.Claims.FirstOrDefault(x => x.Type == "User").Value;
               

                return new
                {
                    success = false,
                    message = "Verificar Si estas Enviando un Token Valido",
                    Results = ""
                };

            }
            catch (Exception ex)
            {
                return new
                {
                    success = false,
                    message = "Catch" + ex.Message,
                    Results = ""
                };
            }
        }
    }
}
