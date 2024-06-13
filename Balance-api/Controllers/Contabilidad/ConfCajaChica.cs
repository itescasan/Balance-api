using Azure;
using Balance_api.Class;
using Balance_api.Contexts;
using Balance_api.Models.Sistema;
using Balance_api.Models.Contabilidad;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Transactions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using DevExpress.Pdf.Interop;
using DevExpress.CodeParser;
namespace Balance_api.Controllers.Contabilidad
{
    public class ConfCajaChica : Controller
    {
        private readonly BalanceEntities Conexion;

        public ConfCajaChica(BalanceEntities db)
        {
            Conexion = db;
        }

        [Route("api/Contabilidad/ConfCajaChica")]
        [HttpGet]
        public string ConfCaja_Chica(string user)
        {
            return V_ConfCajaChica(user);
        }

        private string V_ConfCajaChica(string user)
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

                    Cls_Datos datos = new Cls_Datos();                  


                    var qConfCajaChica = Conexion.ConfCaja.ToList();


                    datos = new Cls_Datos();
                    datos.Nombre = "CUENTAS CAJA";
                    datos.d = qConfCajaChica;
                    lstDatos.Add(datos);

                    var qCuentasC = (from _q in Conexion.CatalogoCuenta
                                     where _q.Nivel == 5 && _q.IdGrupo == 5
                                     select new
                                     {
                                         _q.CuentaContable,
                                         _q.NombreCuenta,
                                         Valor = 0.0,
                                         Activo = true                                    
                                        
                                        
                                     }).ToList();

                    datos = new Cls_Datos();
                    datos.Nombre = "CUENTAS CAJA";
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

        [Route("api/Contabilidad/GuardarConfCajaChica")]
        [HttpPost]

        public IActionResult GuardarConfCajaChica([FromBody] ConfCaja[] d)
        {
            if (ModelState.IsValid)
            {

                return Ok(V_GuardarConfCajaChica(d));

            }
            else
            {
                return BadRequest();
            }
        }

        private string V_GuardarConfCajaChica(ConfCaja[] d)
        {
            string json = string.Empty;
            string msg = string.Empty;
            try
            {
                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                using (Conexion)
                {
                    foreach (ConfCaja f in d)
                    {
                        bool esNuevo = false;
                        ConfCaja? a = Conexion.ConfCaja.Find(f.IdTecho);
                        

                        if (a == null)
                        {
                            a = new ConfCaja();
                            esNuevo = true;
                            msg = "Registro Guardado";
                        }
                        else
                        {
                             msg = "Registro Aptualizado"; 
                        }                  

                        a.CuentaContable = f.CuentaContable;
                        a.Valor = f.Valor;
                        a.Nombre = f.Nombre;    
                        a.Estado = f.Estado;
                        a.Serie = f.Serie;
                        a.Consecutivo = f.Consecutivo;  

                        if (esNuevo) Conexion.ConfCaja.Add(a);
                        Conexion.SaveChanges();
                    }




                    Cls_Datos datos = new Cls_Datos();
                    datos.Nombre = "TECHO CAJA CHICA";
                    datos.d = msg;





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

