using Azure;
using Balance_api.Class;
using Balance_api.Contexts;
using Balance_api.Models.Sistema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Transactions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Balance_api.Controllers.Sistema
{
    //[Produces("application/json")]
    public class ServerController : Controller
    {

        private readonly BalanceEntities Conexion;

        public ServerController(BalanceEntities db)
        {
            Conexion = db;
        }
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


                    var qUsuario = (from _q in Conexion.Usuarios
                                    where _q.Activo == true && _q.Usuario.Equals(user)
                                    select new
                                    {
                                        User = _q.Usuario,
                                        Nombre = string.Concat(_q.Nombres, " ", _q.Apellidos),
                                        Pwd = _q.Pass,
                                        Rol = string.Empty,
                                        FechaLogin =  string.Format("{0:yyyy-MM-dd hh:mm:ss}", DateTime.Now),
                                        Desconectar = !_q.AccesoWeb ? true : _q.Desconectar
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
                        json = Cls_Mensaje.Tojson(null, 0, string.Empty, "Contraseña Incorrecta.", 1);
                        return json;
                    }

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
            datos2.d = Desconectar ? "-1" : "7200";


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
                                        select  new { _q.IdSerie, _q.DescripcionSerie }
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
                    foreach(AccesoWeb f in d)
                    {
                        bool esNuevo = false;
                        AccesoWeb? a = Conexion.AccesoWeb.Find(f.IdAcceso);

                        if (a == null)
                        {
                            a = new AccesoWeb();
                            esNuevo = true;
                        }

                        a.EsMenu = f.EsMenu;
                        a.Id = f.Id;
                        a.Caption = f.Caption;
                        a.MenuPadre = f.MenuPadre;
                        a.Clase = f.Clase;
                        a.Usuario = f.Usuario;
                        a.Modulo = f.Modulo;
                        a.Activo = f.Activo;

                        if (esNuevo) Conexion.AccesoWeb.Add(a);
                        Conexion.SaveChanges();
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


    }
}
