using Azure;
using Balance_api.Class;
using Balance_api.Contexts;
using Balance_api.Models.Sistema;
using Balance_api.Models.Contabilidad;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Transactions;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace Balance_api.Controllers.Contabilidad
{
    public class AccesoCajaCh : Controller
    {
        private readonly BalanceEntities Conexion;

        public AccesoCajaCh(BalanceEntities db)
        {
            Conexion = db;
        }

        [Route("api/Contabilidad/AccesoCajaChica")]
        [HttpGet]
        public string AccesoCaja_Chica(string user)
        {
            return V_AccesoCajaChica(user);
        }

        private string V_AccesoCajaChica(string user)
        {
            string json = string.Empty;
            try
            {
                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                using (Conexion)
                {


                    Usuarios? Usuario = Conexion.Usuarios.FirstOrDefault(f => f.Usuario == user && f.AccesoWeb);

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




                    var qAccesoCajaChica = Conexion.AccesoCajaChica.ToList();


                    datos = new Cls_Datos();
                    datos.Nombre = "ACCESO CAJA CHICA";
                    datos.d = qAccesoCajaChica;
                    lstDatos.Add(datos);

                    var lista1 = (
                         from cc in Conexion.CatalogoCuenta
                         where cc.Nivel == 5 && cc.IdGrupo == 5
                         select new
                         {
                             IdAcceso = 0,
                             CuentaContable = cc.CuentaContable,
                             Usuario = "",
                             Activo = false,
                             Clase = "fa-solid fa-house",
                             NombreCuenta = cc.NombreCuenta
                         }
                     ).ToList();

                    var lista2 = (
                        from ca in Conexion.CajasAsociadas
                        select new
                        {
                            IdAcceso = ca.IdAcceso,
                            CuentaContable = ca.CuentaContable,
                            Usuario = ca.Usuario,
                            Activo = false,
                            Clase = "fa-solid fa-house",
                            NombreCuenta = ca.NombreCuenta
                        }
                    ).ToList();

                    var qCuentasC = lista2.Union(lista1).ToList();

                    datos = new Cls_Datos();
                    datos.Nombre = "CUENTAS CAJA CHICA";
                    datos.d = qCuentasC;
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

        [Route("api/Contabilidad/GuardarAccesoCajaChica")]
        [HttpPost]
        public IActionResult GuardarAccesoCajaChica([FromBody] AccesoCajaC[] d)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            return Ok(V_GuardarAccesoCajaChica(d));
        }

        private string V_GuardarAccesoCajaChica(AccesoCajaC[] d)
        {
            string json = string.Empty;
            string msg = string.Empty;

            try
            {
                using TransactionScope scope = new(TransactionScopeOption.Required,
                    new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });

                using (Conexion)
                {
                    // 🔹 Obtenemos todas las cuentas existentes una sola vez
                    var existentes = Conexion.AccesoCajaChica.ToList();
                    var ConfCc = Conexion.ConfCaja.ToList();

                    foreach (var f in d)
                    {
                        // Buscar coincidencia insensible a mayúsculas/minúsculas y espacios
                        var a = existentes.FirstOrDefault(x =>
                            x.CuentaContable.Trim().ToUpper() == f.CuentaContable.Trim().ToUpper() &&
                            x.NombreCuenta.Trim().ToUpper() == f.NombreCuenta.Trim().ToUpper() && x.Usuario.Trim().ToUpper() == f.Usuario.Trim().ToUpper());

                        var b = ConfCc.FirstOrDefault(x => x.Nombre.Trim().ToUpper() == f.NombreCuenta.Trim().ToUpper());

                        if (a == null)
                        {
                            // 🔸 Nuevo registro
                            a = new AccesoCajaC
                            {
                                CuentaContable = f.CuentaContable.Trim(),
                                NombreCuenta = f.NombreCuenta.Trim(),
                                Usuario = f.Usuario,
                                Activo = f.Activo,
                                Serie = b!.Serie
                            };

                            Conexion.AccesoCajaChica.Add(a);
                        }
                        else
                        {
                            // 🔹 Ya existe, actualizar solo lo necesario
                            a.Usuario = f.Usuario;
                            a.Activo = f.Activo;
                            a.Serie = b!.Serie;
                        }
                    }

                    // ✅ Guardamos una sola vez
                    Conexion.SaveChanges();
                    scope.Complete();

                    msg = (d.Length > 1 ? "Registros procesados correctamente" : "Registro actualizado");

                    var datos = new Cls_Datos
                    {
                        Nombre = "ACCESO CAJA CHICA",
                        d = msg
                    };

                    json = Cls_Mensaje.Tojson(datos, 1, string.Empty, string.Empty, 0);
                }
            }
            catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;
        }



        //    [Route("api/Contabilidad/GuardarAccesoCajaChica")]
        //    [HttpPost]

        //    public IActionResult GuardarAccesoCajaChica([FromBody] AccesoCajaC[] d)
        //    {
        //        if (ModelState.IsValid)
        //        {

        //            return Ok(V_GuardarAccesoCajaChica(d));

        //        }
        //        else
        //        {
        //            return BadRequest();
        //        }
        //    }

        //    private string V_GuardarAccesoCajaChica(AccesoCajaC[] d)
        //    {
        //        string json = string.Empty;
        //        string msg = string.Empty;
        //        try
        //        {


        //            using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
        //            using (Conexion)

        //            {
        //                foreach (AccesoCajaC f in d)
        //                {
        //                    bool esNuevo = false;
        //                    AccesoCajaC? a = Conexion.AccesoCajaChica.Find(f.IdAcceso);

        //                    if (a == null)
        //                    {
        //                        a = new AccesoCajaC();
        //                        esNuevo = true;
        //                        msg = "Registro Guardado";
        //                        a.CuentaContable = f.CuentaContable;
        //                        a.NombreCuenta = f.NombreCuenta;
        //                        a.Usuario = f.Usuario;
        //                        a.Activo = f.Activo;
        //                    }
        //                    else
        //                    {
        //                        msg = "Registro Aptualizado";
        //                        a.CuentaContable = f.CuentaContable;
        //                        a.NombreCuenta = f.NombreCuenta;
        //                        a.Usuario = f.Usuario;
        //                        a.Activo = f.Activo;

        //                        if (esNuevo) Conexion.AccesoCajaChica.Add(a);
        //                    }


        //                    Conexion.SaveChanges();
        //                }




        //                Cls_Datos datos = new Cls_Datos();
        //                datos.Nombre = "ACCESO CAJA CHICA";
        //                datos.d = msg;





        //                scope.Complete();

        //                json = Cls_Mensaje.Tojson(datos, 1, string.Empty, string.Empty, 0);



        //            }



        //        }
        //        catch (Exception ex)
        //        {
        //            json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
        //        }

        //        return json;

        //    }
    }
}
