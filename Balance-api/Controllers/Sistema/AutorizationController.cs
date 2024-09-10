using Azure;
using Balance_api.Class;
using Balance_api.Contexts;
using Balance_api.Models.Sistema;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Transactions;
using static DevExpress.Xpo.Helpers.AssociatedCollectionCriteriaHelper;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Balance_api.Models.Custom;
using Balance_api.Services;
using System.IdentityModel.Tokens.Jwt;


namespace Balance_api.Services
{
    public class AutorizationController : Controller
    {


        private readonly IAutorizacionService _autorizacionService;
        private readonly BalanceEntities Conexion;
        //[Authorize]
        [Route("api/Sistema/Autorization")]
        [HttpGet]
        public IActionResult Autorize(string user, string pass, string Modulo)
        {
            return Ok(v_Autorize(user, pass));
        }

        private string v_Autorize(string user, string pass)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new();
                    Usuarios _u = Conexion.Usuarios.FirstOrDefault(f => f.Usuario.TrimStart().TrimEnd() == user)!;

                    var qUsuario = (from _q in Conexion.Usuarios
                                    where _q.Activo == true && _q.Usuario.Equals(user)
                                    select new
                                    {
                                        User = _q.Usuario,
                                        Nombre = string.Concat(_q.Nombres, " ", _q.Apellidos),
                                        Pwd = Conexion.Database.SqlQuery<string>($"SELECT [SIS].[Desencriptar]({"0x" + BitConverter.ToString(_q.Pass).Replace("-", "")})").Single(),
                                        Rol = string.Empty,
                                        FechaLogin = string.Format("{0:yyyy-MM-dd hh:mm:ss}", DateTime.Now),
                                        Desconectar = !_q.AccesoWeb ? true : _q.Desconectar,
                                        Token = new AutorizacionResponse()
                                    }).ToList();



                    if (qUsuario.Count == 0)
                    {
                        json = Cls_Mensaje.Tojson(null, 0, "1", "Usuario no encontrado.", 1);
                        return json;
                    }

                    string Pwd = qUsuario[0].Pwd;

                    if (!Pwd.Equals(pass))
                    {
                        if (_u.Intento == null) _u.Intento = 0;
                        _u.Intento += 1;
                        Conexion.SaveChanges();

                        if (_u.Intento >= 3)
                        {
                            json = Cls_Mensaje.TojsonT(null, 0, string.Empty, $"<b>Usuario Bloqueado</b>", 1, null);
                            return json;
                        }

                        json = Cls_Mensaje.TojsonT(null, 0, string.Empty, $"<span>Contraseña Incorrecta. <br>Intento Restante: <b>{(3 - _u.Intento).ToString()}</b</span>", 1, null);
                        return json;

                    }


                    if (_u.Intento >= 3)
                    {
                        if (_u.Intento == null) _u.Intento = 0;


                        json = Cls_Mensaje.TojsonT(null, 0, string.Empty, "<b>Usuario Bloqueado.</b>", 1, null);
                        return json;
                    }


                    _u.Intento = 0;
                    Conexion.SaveChanges();

                    object[] o = { string.Empty, string.Empty, 0 };
                   // o = Auntenticar();



                    //Cls_Token tk = new Cls_Token();
                    //tk.access_token = o[0].ToString();
                    //tk.refresh_token = o[1].ToString();
                    //tk.expires_in = Convert.ToInt32(o[2]);

                    //qUsuario[0].Token = tk;



                    Cls_Datos datos = new();
                    datos.Nombre = "USUARIO";
                    datos.d = qUsuario;
                    lstDatos.Add(datos);                  


                    json = Cls_Mensaje.Tojson(lstDatos, lstDatos.Count, string.Empty, string.Empty, 0);
                }



            }
            catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;
        }

        public async Task<IActionResult> Auntenticar([FromBody] AutorizacionRequest autorizacion)
        {
            var resultado_autorizacion = await _autorizacionService.DevolverToken(autorizacion);
            if (resultado_autorizacion == null)
                return Unauthorized();
            return Ok(resultado_autorizacion);

        }

    }
}
