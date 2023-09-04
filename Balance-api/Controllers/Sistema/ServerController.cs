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
            return v_Login(user, pass);
        }

        private string v_Login(string user, string pass)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new List<Cls_Datos>();


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

                    Cls_Datos datos = new Cls_Datos();
                    datos.Nombre = "USUARIO";
                    datos.d = qUsuario;
                    lstDatos.Add(datos);


                    lstDatos.AddRange(v_FechaServidor(user, qUsuario[0].Desconectar));

              

                    json = Cls_Mensaje.Tojson(lstDatos, lstDatos.Count, string.Empty, string.Empty, 0);
                }



            }
            catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;
        }


        [Route("api/Sistema/FechaServidor")]
        [HttpGet]
        public string FechaServidor(string user)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new List<Cls_Datos>();
                    Usuarios? u = Conexion.Usuarios.FirstOrDefault(f => f.Usuario.Equals(user));
         

                    lstDatos.AddRange(v_FechaServidor(user, (u == null ? true : u.Desconectar) ));



                    json = Cls_Mensaje.Tojson(lstDatos, lstDatos.Count, string.Empty, string.Empty, 0);
                }



            }
            catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;
        }

        private Cls_Datos[] v_FechaServidor(string user, bool Desconectar)
        { 
     
            Cls_Datos datos = new Cls_Datos();
            datos.Nombre = "FECHA SERVIDOR";
            datos.d = string.Format("{0:yyy-MM-dd hh:mm:ss}", DateTime.Now);


            Cls_Datos datos2 = new Cls_Datos();
            datos2.Nombre = "DESCONECCION";
            datos2.d = Desconectar ? "-1" : "7200";

           
            return new Cls_Datos[] { datos, datos2 };
        }



        [Route("api/Sistema/TC")]
        [HttpGet]
        public string TC(DateTime f)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new List<Cls_Datos>();
                    Usuarios? u = Conexion.Usuarios.FirstOrDefault(f => f.Usuario.Equals(user));


                    lstDatos.AddRange(v_FechaServidor(user, (u == null ? true : u.Desconectar)));



                    json = Cls_Mensaje.Tojson(lstDatos, lstDatos.Count, string.Empty, string.Empty, 0);
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
