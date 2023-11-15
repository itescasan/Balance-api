using Azure;
using Balance_api.Class;
using Balance_api.Contexts;
using Balance_api.Models.Sistema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public string Login(string user, string pass)
        {
            return V_Login(user, pass);
        }

        private string V_Login(string user, string pass)
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
                                        _q.Desconectar
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


                    lstDatos.AddRange(V_DatosServidor(user, qUsuario[0].Desconectar));

              

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
        public string DatosServidor(string user)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new();
                    Usuarios? u = Conexion.Usuarios.FirstOrDefault(f => f.Usuario.Equals(user));
         

                    lstDatos.AddRange(V_DatosServidor(user, (u == null ? true : u.Desconectar)));
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

        private Cls_Datos[] V_DatosServidor(string user, bool Desconectar)
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


            


            return new Cls_Datos[] { datos, datos2, datos3 };
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
                                        select _q.IdSerie
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

                        case "Contabilidad":
                            var qCon = (from _q in Conexion.SerieDocumento
                                        where _q.IdSerie == Serie
                                        select string.Concat(_q.IdSerie, _q.Consecutivo + 1)
                                     ).FirstOrDefault();

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

    }
}
