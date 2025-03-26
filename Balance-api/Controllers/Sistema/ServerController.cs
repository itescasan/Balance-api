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
using System.Net.Mail;
using DevExpress.ClipboardSource.SpreadsheetML;
using System.Net;
using DevExpress.Pdf.Native.BouncyCastle.Utilities.Net;

namespace Balance_api.Controllers.Sistema
{
    //[Produces("application/json")]
    public class ServerController : Controller
    {

        private readonly IAutorizacionService _autorizacionService;
        private readonly BalanceEntities Conexion;

        public ServerController(BalanceEntities db, IAutorizacionService autorizacionService)
        {
            Conexion = db;
            _autorizacionService = autorizacionService;
        }

        //[Authorize]
        [Route("api/Sistema/Login")]
        [HttpGet]
        public string Login(string user, string pass, string Modulo)
        {
            return V_Login(user, pass, Modulo);
        }

        private string V_Login(string user, string pass, string Modulo)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new();
                    Usuarios? _u = Conexion.Usuarios.FirstOrDefault(f => f.Usuario.TrimStart().TrimEnd() == user);


                    List<Cls_Usuario> qUsuario = (from _q in Conexion.Usuarios
                                    where _q.Activo == true && _q.AccesoWeb == true && _q.Usuario.Equals(user)
                                    select new Cls_Usuario()
                                    {
                                        User = _q.Usuario,
                                        Nombre = string.Concat(_q.Nombres, " ", _q.Apellidos),
                                        Pwd = _q.Pass,
                                        Rol = string.Empty,
                                        FechaLogin = string.Format("{0:yyyy-MM-dd hh:mm:ss}", DateTime.Now),
                                        Desconectar = !_q.AccesoWeb ? true : _q.Desconectar,
                                        CON_CodMail = _q.CON_CodMail
                                    }).ToList();



                    if (qUsuario.Count == 0)
                    {
                        json = Cls_Mensaje.Tojson(null, 0, "1", "Usuario no encontrado.", 1);
                        return json;
                    }

                    string sQuery = $"SELECT [SIS].[Desencriptar](  {"0x"}{BitConverter.ToString(qUsuario[0].Pwd).Replace("-", "")}) AS Pass";
                    string Pwd = Conexion.Database.SqlQueryRaw<string>(sQuery).ToList().First();

                    if (!Pwd.Equals(pass))
                    {


                        if (_u?.Intento == null) _u!.Intento = 0;
                        _u.Intento += 1;
                        Conexion.SaveChanges();



                        if (_u.Intento >= 3)
                        {
                            json = Cls_Mensaje.Tojson(null, 0, string.Empty, $"<b>Usuario Bloqueado</b>", 1);
                            return json;
                        }

                        json = Cls_Mensaje.Tojson(null, 0, string.Empty, $"<span>Contraseña Incorrecta. <br>Intento Restante: <b>{(3 - _u.Intento).ToString()}</b</span>", 1);
                        return json;
                    }





                    if (_u?.Intento >= 3)
                    {
                        if (_u.Intento == null) _u.Intento = 0;


                        json = Cls_Mensaje.Tojson(null, 0, string.Empty, "<b>Usuario Bloqueado.</b>", 1);
                        return json;
                    }


                    _u!.Intento = 0;
                    Conexion.SaveChanges();



                    if (_u?.Correo == null || _u.Correo == string.Empty)
                    {
                        json = Cls_Mensaje.Tojson(null, 0, string.Empty, "El usuario no tiene asignado un correo electronico.", 1);
                        return json;
                    }


              

                  /*  if (_u.CON_Mail_Web_Date != Convert.ToDateTime(DateTime.Now.ToShortDateString()))
                    {
                        _u.CON_Mail_Web_Date = null;
                        qUsuario[0].CON_CodMail = string.Empty;
                    }*/



                   // if (_u.CON_Mail_Web_Date == null)
                   // {


                        Random ran = new Random();

                    string b = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                    string sc = "!@#$%^&*~";

                        int length = 6;

                        string random = "";

                        for (int i = 0; i < length; i++)
                        {
                            int a = ran.Next(b.Length); //string.Lenght gets the size of string
                            random = random + b.ElementAt(a);
                        }
                       /* for (int j = 0; j < 2; j++)
                        {
                            int sz = ran.Next(sc.Length);
                            random = random + sc.ElementAt(sz);
                        }*/



                        _u.CON_Mail_Web = random.ToUpper();
                        _u.CON_Mail_Web_Date = DateTime.Now;
                        _u.CON_CodMail = string.Empty;

                        string Correo = _u.Correo == null ? string.Empty : _u.Correo;


                        MailMessage mail = new MailMessage();
                        mail.From = new MailAddress("info@escasan.com.ni");
                        


                    switch(Modulo)
                    {
                        case "CON":
                            mail.Subject = $"ESCASAN ACCESO CONTABILIDAD";
                            mail.Body = $"Codigo Acceso Modulo Contable <br><div style='width:100%;text-aling:center'><b style='font-size: 18px'>{_u.CON_Mail_Web}<b></div>";
                            break;

                        default:
                            json = Cls_Mensaje.Tojson(null, 0, string.Empty, "Modulo no configurado.", 1);
                            return json;
                    }





                       
                        mail.IsBodyHtml = true;
                        mail.To.Add(Correo);

                        SmtpClient smtpClient = new SmtpClient("smtp.office365.com");
                        NetworkCredential nameAndPassword = new NetworkCredential("info@escasan.com.ni", "8hSTdrupcEsassfuPYuTS8x4X");

                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        smtpClient.Port = 587;
                        smtpClient.Credentials = nameAndPassword;
                        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtpClient.EnableSsl = true;
                        smtpClient.Send(mail);






                        Conexion.SaveChanges();





                  //  }
            
                    

                    Cls_Datos datos = new();
                    datos.Nombre = "USUARIO";
                    datos.d = qUsuario;
                    lstDatos.Add(datos);

                    lstDatos.AddRange(V_DatosServidor(user, qUsuario[0].Desconectar, Modulo));


                    json = Cls_Mensaje.Tojson(lstDatos, lstDatos.Count, string.Empty, string.Empty, 0);
                }



            }
            catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;
        }


        //[Authorize]
        [Route("api/Sistema/ValidarCodigo")]
        [HttpGet]
        public string ValidarCodigo(string user,  string cod, string Modulo)
        {
            return V_ValidarCodigo(user,  cod, Modulo);
        }


        private string V_ValidarCodigo(string user,  string cod, string Modulo)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
       
                    Usuarios _u = Conexion.Usuarios.FirstOrDefault(f => f.Usuario == user)!;

                    if(_u  == null)
                    {
                        json = Cls_Mensaje.Tojson(null, 0, string.Empty, $"<span>Usuario no valido.</span>", 1, null);
                        return json;
                    }


                    switch (Modulo)
                    {
                        case "CON":

                            if (_u.CON_Mail_Web != cod)
                            {

                                json = Cls_Mensaje.Tojson(null, 0, string.Empty, $"<span>Codigo Invalido.</span>", 1, null);
                                return json;
                            }


                            if (_u.CON_Mail_Web_Date != Convert.ToDateTime(DateTime.Now.ToShortDateString()))
                            {

                                json = Cls_Mensaje.Tojson(null, 0, string.Empty, $"<span>Codigo Expirado.</span>", 1, null);
                                return json;
                            }


                            _u.CON_CodMail = cod;
                            Conexion.SaveChanges();


                            break;

                        default:
                            json = Cls_Mensaje.Tojson(null, 0, string.Empty, $"<span>Modulo no configurado.</span>", 1, null);
                            return json;
                    }


                   


                   





                    Cls_Datos datos = new();
                    datos.Nombre = "COD";
                    datos.d = cod;
      

                    json = Cls_Mensaje.Tojson(datos, 1, string.Empty, string.Empty, 0);
                }



            }
            catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;
        }







        [Route("api/Sistema/DatosServidor")]
        [HttpGet]
        public string DatosServidor(string user, string Modulo)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new();
                    Usuarios? u = Conexion.Usuarios.FirstOrDefault(f => f.Usuario.Equals(user));


                    lstDatos.AddRange(V_DatosServidor(user, (u == null ? true : !u.AccesoWeb ? true : u.Desconectar), Modulo));
                    lstDatos.Add(V_TC(DateTime.Now));



                    json = Cls_Mensaje.Tojson(lstDatos, lstDatos.Count, string.Empty, string.Empty, 0);
                }



            }
            catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;
        }

        private Cls_Datos[] V_DatosServidor(string user, bool Desconectar, string Modulo)
        {

            Cls_Datos datos = new();
            datos.Nombre = "FECHA SERVIDOR";
            datos.d = string.Format("{0:yyy-MM-dd hh:mm:ss}", DateTime.Now);


            Cls_Datos datos2 = new();
            datos2.Nombre = "DESCONECCION";
            datos2.d = Desconectar ? "-1" : "14400";


            string IdMonedaLocal = Conexion.Database.SqlQueryRaw<string>($"SELECT TOP 1 MonedaLocal FROM SIS.Parametros").ToList().First();
            Cls_Datos datos3 = new();
            datos3.Nombre = "MONEDA LOCAL";
            datos3.d = IdMonedaLocal;



            var Perfil = Conexion.AccesoWeb.Where(w => w.Usuario == user && w.Modulo == Modulo && w.Activo).ToList();

            Cls_Datos datos4 = new Cls_Datos();
            datos4.Nombre = "PERFIL";
            datos4.d = Perfil;



            return new Cls_Datos[] { datos, datos2, datos3, datos4 };
        }



        [Route("api/Sistema/TC")]
        [HttpGet]
        public string TC(DateTime f)
        {
            string json;
            try
            {

                json = Cls_Mensaje.Tojson(V_TC(f), 1, string.Empty, string.Empty, 0);

            }
            catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;
        }


        private Cls_Datos V_TC(DateTime f)
        {
            Cls_Datos Datos = new();
            Datos.Nombre = "TC";
            Datos.d = 0;


            using (Conexion)
            {

                string sQuery = $"SELECT TasaCambio FROM CON.TasaCambio WHERE fecha = CAST('{string.Format("{0:yyyy-MM-dd}", f)}' AS DATE)";
                decimal TC = 0;
                List<decimal> lst = Conexion.Database.SqlQueryRaw<decimal>(sQuery).ToList();

                if (lst.Count > 0) TC = lst.First();

                Datos.d = TC;


            }

            return Datos;

        }


        [Route("api/Sistema/Serie")]
        [HttpGet]
        public string GetSerie(string CodBodega, string Tipo)
        {
            return V_GetSerie(CodBodega, Tipo);
        }

        private string V_GetSerie(string CodBodega, string Tipo)
        {
            string json = string.Empty;


            Cls_Datos datos = new();
            datos.Nombre = "SERIE";


            try
            {

                using (Conexion)
                {


                    switch (Tipo)
                    {
                        case "Importacion":

                            var qSerie = (from _q in Conexion.Serie
                                          join _b in Conexion.BodegaSerie on _q.IdSerie equals _b.Serie
                                          where _b.CodBodega == CodBodega && _b.EsImport
                                          group _q by new { _q.IdSerie } into g
                                          select new
                                          {
                                              g.Key.IdSerie
                                          }
                                      ).ToList();

                            datos.d = qSerie;

                            break;

                        case "Inventario":

                            var qInventario = (from _q in Conexion.Serie
                                               join _b in Conexion.BodegaSerie on _q.IdSerie equals _b.Serie
                                               where _b.CodBodega == CodBodega && _b.EsInv
                                               group _q by new { _q.IdSerie } into g
                                               select new
                                               {
                                                   g.Key.IdSerie
                                               }
                                      ).ToList();



                            datos.d = qInventario;

                            break;


                        case "PedidoInventario":

                            var qPedidoInventario = (from _q in Conexion.Serie
                                                     join _b in Conexion.BodegaSerie on _q.IdSerie equals _b.Serie
                                                     where _b.CodBodega == CodBodega && _b.EsPedido
                                                     group _q by new { _q.IdSerie } into g
                                                     select new
                                                     {
                                                         g.Key.IdSerie
                                                     }
                                      ).ToList();


                            datos.d = qPedidoInventario;

                            break;

                        case "Requisa":

                            var qRequisa = (from _q in Conexion.Serie
                                            join _b in Conexion.BodegaSerie on _q.IdSerie equals _b.Serie
                                            where _b.CodBodega == CodBodega && _b.EsRequisa
                                            group _q by new { _q.IdSerie } into g
                                            select new
                                            {
                                                g.Key.IdSerie
                                            }
                                      ).ToList();


                            datos.d = qRequisa;

                            break;



                        case "Otros":

                            var qOtros = (from _q in Conexion.Serie
                                          join _b in Conexion.BodegaSerie on _q.IdSerie equals (_b.SerieUnion == null ? _b.Serie : _b.SerieUnion)
                                          where _b.CodBodega == CodBodega && _b.EsImport
                                          group _q by new { _q.IdSerie } into g
                                          select new
                                          {
                                              g.Key.IdSerie
                                          }
                                      ).ToList();


                            datos.d = qOtros;

                            break;

                        case "Contabilidad":
                            var qCon = (from _q in Conexion.SerieDocumento
                                        where !_q.TipoDocumento.Automatico && _q.Activo
                                        select new { _q.IdSerie, _q.DescripcionSerie }
                                   ).ToList();

                            datos.d = qCon;
                            break;
                    }
                }


                json = Cls_Mensaje.Tojson(datos, 1, string.Empty, string.Empty, 0);


            }
            catch
            {
                json = Cls_Mensaje.Tojson(null, 0, "-1", string.Empty, 1);
            }

            return json;
        }


        [Route("api/Sistema/Consecutivo")]
        [HttpGet]
        public string GetConsecutivo(string Serie, string Tipo)
        {
            return V_GetConsecutivo(Serie, Tipo);
        }

        private string V_GetConsecutivo(string Serie, string Tipo)
        {
            string json = string.Empty;

            Cls_Datos datos = new();
            datos.Nombre = "CONSECUTIVO";



            try
            {

                using (Conexion)
                {


                    switch (Tipo)
                    {
                        case "Importacion":

                            var qImportacion = (from _q in Conexion.Serie
                                                where _q.IdSerie == Serie
                                                select string.Concat(_q.IdSerie, _q.Inventario + 1)
                                      ).FirstOrDefault();


                            datos.d = qImportacion;

                            break;

                        case "Inventario":

                            var qInventario = (from _q in Conexion.Serie
                                               where _q.IdSerie == Serie
                                               select string.Concat(_q.IdSerie, _q.Inventario + 1)
                                      ).FirstOrDefault();


                            datos.d = qInventario;

                            break;


                        case "PedidoInventario":

                            var qPedidoInventario = (from _q in Conexion.Serie
                                                     where _q.IdSerie == Serie
                                                     select string.Concat(_q.IdSerie, _q.Pedido + 1)
                                      ).FirstOrDefault();


                            datos.d = qPedidoInventario;

                            break;


                        case "Requisa":

                            var qRequisa = (from _q in Conexion.Serie
                                            where _q.IdSerie == Serie
                                            select string.Concat(_q.IdSerie, _q.Requisa + 1)
                                      ).FirstOrDefault();


                            datos.d = qRequisa;

                            break;

                        case "OT":

                            var qOT = (from _q in Conexion.Serie
                                       where _q.IdSerie == Serie
                                       select string.Concat(_q.IdSerie, _q.OT + 1)
                                      ).FirstOrDefault();


                            datos.d = qOT;

                            break;

                        case "Anexo":

                            var qAnx = (from _q in Conexion.Serie
                                        where _q.IdSerie == Serie
                                        select string.Concat(_q.IdSerie, _q.Anexos + 1)
                                      ).FirstOrDefault();


                            datos.d = qAnx;

                            break;



                    }


                }

                json = Cls_Mensaje.Tojson(datos, 1, string.Empty, string.Empty, 0);

            }
            catch
            {
                json = Cls_Mensaje.Tojson(null, 0, "-1", string.Empty, 1);
            }

            return json;
        }


        [Route("api/Sistema/ConsecutivoContabilidad")]
        [HttpGet]
        public string GetConsecutivoContabilidad(string Serie, DateTime Fecha)
        {
            return V_GetConsecutivoContabilidad(Serie, Fecha);
        }

        private string V_GetConsecutivoContabilidad(string Serie, DateTime Fecha)
        {
            string json = string.Empty;

            Cls_Datos datos = new();
            datos.Nombre = "CONSECUTIVO";



            try
            {

                using (Conexion)
                {

                    var qContabilidad = (from _q in Conexion.ConsecutivoDiario
                                         where _q.IdSerie == Serie && _q.Mes == Fecha.Month && _q.Anio == Fecha.Year
                                         select string.Concat(Serie, "$-", _q.Consecutivo + 1)
                              ).FirstOrDefault();


                    datos.d = qContabilidad;

                }

                json = Cls_Mensaje.Tojson(datos, 1, string.Empty, string.Empty, 0);

            }
            catch
            {
                json = Cls_Mensaje.Tojson(null, 0, "-1", string.Empty, 1);
            }

            return json;
        }





        [Route("api/Sistema/CerrarSession")]
        [HttpPost]
        public IActionResult CerrarSession(string user)
        {

            if (ModelState.IsValid)
            {

                return Ok(V_CerrarSession(user));

            }
            else
            {
                return BadRequest();
            }

        }

        private string V_CerrarSession(string user)
        {

            string json = string.Empty;

            try
            {


                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                using (Conexion)
                {

                    Usuarios? _u = Conexion.Usuarios.FirstOrDefault(f => f.Usuario == user);

                    if(_u != null)
                    {
                        _u.CON_CodMail = string.Empty;
                        _u.CON_Mail_Web = string.Empty;
                        _u.CON_Mail_Web_Date = null;
                        Conexion.SaveChanges();
                    }
                  

                    Cls_Datos datos = new Cls_Datos();
                    datos.Nombre = "CERRAR";
                    datos.d = "Session Cerrada";





                    scope.Complete();

                    json = Cls_Mensaje.Tojson(datos, 1, string.Empty, string.Empty, 0);



                }



            }
            catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;

        }





        [Route("api/Sistema/AccesoWeb")]
        [HttpGet]
        public string AccesoWeb(string user)
        {
            return V_AccesoWeb(user);
        }

        private string V_AccesoWeb(string user)
        {
            string json = string.Empty;
            try
            {
                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                using (Conexion)
                {

                    Usuarios? Usuario = Conexion.Usuarios.FirstOrDefault(f => f.Usuario == user && f.IdRol == 1 & f.AccesoWeb);

                    if (Usuario == null)
                    {
                        json = Cls_Mensaje.Tojson(null, 0, "1", "No tiene permiso para acceder a la inforacion solicitada.", 1);
                        return json;
                    }



                    List<Cls_Datos> lstDatos = new List<Cls_Datos>();


                    var qUsuario = (from _q in Conexion.Usuarios
                                    where _q.AccesoWeb
                                    select new
                                    {
                                        _q.Usuario,
                                        Nombre = string.Concat(_q.Usuario, " --> ", _q.Nombres, " ", _q.Apellidos)
                                    }).ToList();






                    Cls_Datos datos = new Cls_Datos();
                    datos.Nombre = "USUARIO";
                    datos.d = qUsuario;
                    lstDatos.Add(datos);



                    var qAccesoWeb = Conexion.AccesoWeb.ToList();


                    datos = new Cls_Datos();
                    datos.Nombre = "ACCESO WEB";
                    datos.d = qAccesoWeb;
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


        [Route("api/Sistema/GuardarAcceso")]
        [HttpPost]
        public IActionResult GuardarAcceso([FromBody] AccesoWeb[] d)
        {

            if (ModelState.IsValid)
            {

                return Ok(V_GuardarAcceso(d));

            }
            else
            {
                return BadRequest();
            }

        }

        private string V_GuardarAcceso(AccesoWeb[] d)
        {

            string json = string.Empty;

            try
            {


                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                using (Conexion)
                {
                    int i = 0;
                    foreach (AccesoWeb f in d)
                    {
                        bool esNuevo = false;
                        AccesoWeb? a = Conexion.AccesoWeb.FirstOrDefault(w => w.Id == f.Id && w.Modulo == f.Modulo  && w.Usuario == f.Usuario);


                        if (i == 0)
                        {
                            Usuarios u = Conexion.Usuarios.FirstOrDefault(w => w.Usuario == f.Usuario)!;
                            u.Intento = 0;
                            Conexion.SaveChanges();
                        }


                        if (a == null)
                        {
                            a = new AccesoWeb();
                            esNuevo = true;
                        }

                        a.EsMenu = f.EsMenu;
                        a.EsSubMenu = f.EsSubMenu;
                        a.Id = f.Id;
                        a.Caption = f.Caption;
                        a.MenuPadre = f.MenuPadre;
                        a.Clase = f.Clase;
                        a.Usuario = f.Usuario;
                        a.Modulo = f.Modulo;
                        a.Activo = f.Activo;

                        if (esNuevo) Conexion.AccesoWeb.Add(a);
                        Conexion.SaveChanges();

                        i++;
                    }




                    Cls_Datos datos = new Cls_Datos();
                    datos.Nombre = "ACCESO WEB";
                    datos.d = "Registro Guardado";





                    scope.Complete();

                    json = Cls_Mensaje.Tojson(datos, 1, string.Empty, string.Empty, 0);



                }



            }
            catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;

        }

        [Route("api/Sistema/Auntenticar")]
        [HttpPost]
        public async Task<IActionResult> Auntenticar([FromBody] AutorizacionRequest autorizacion)
        {
            var resultado_autorizacion = await _autorizacionService.DevolverToken(autorizacion);
            if (resultado_autorizacion == null)
                return Unauthorized();
            return Ok(resultado_autorizacion);

        }

        [Route("api/Sistema/ObtenerRefreshToken")]
        [HttpPost]
        public async Task<IActionResult> ObtenerRefreshToken([FromBody] RefreshTokenRequest request)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenExpiradoSupuestamente = tokenHandler.ReadJwtToken(request.TokenExpirado);

            if (tokenExpiradoSupuestamente.ValidTo > DateTime.UtcNow)            
                return BadRequest(new AutorizacionResponse { Resultado = false, Msg = "Token no expirado" });

           string Idusuario = tokenExpiradoSupuestamente.Claims.First(x =>
           x.Type == JwtRegisteredClaimNames.NameId).Value.ToString();

            var autorizacionResponse = await _autorizacionService.DevolverRefreshToken(request, int.Parse(Idusuario));

            if (autorizacionResponse.Resultado)            
                return Ok(autorizacionResponse);            
            else
                return BadRequest(autorizacionResponse);
        }
    }
        
}
