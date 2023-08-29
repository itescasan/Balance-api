using Balance_api.Class;
using Balance_api.Contexts;
using Balance_api.Models.Contabilidad;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Balance_api.Controllers.Contabilidad
{
   // [Route("api/Contabilidad/[Controller]")]
    public class CatalogoCuentaController : Controller
    {

        private readonly BalanceEntities Conexion;

        public CatalogoCuentaController(BalanceEntities db)
        {
            Conexion = db;
        }


        [Route("api/Contabilidad/CatalogoCuenta/Get")]
        [HttpGet]
        public  string Get()
        {
            return  v_Get();
        }

        private string v_Get()
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new List<Cls_Datos>();

                    var datos = v_Obterner_Cuentas();
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

        private Cls_Datos v_Obterner_Cuentas()
        {

            var qCuentas = (from _q in Conexion.CatalogoCuenta
                            select new
                            {
                                _q.CuentaContable,
                                _q.NombreCuenta,
                                _q.Nivel,
                                _q.IdGrupo,
                                Grupo = _q.GruposCuentas.Nombre,
                                _q.ClaseCuenta,
                                _q.CuentaPadre,
                                _q.Naturaleza,
                                _q.Bloqueada,
                                Filtro = string.Concat(_q.CuentaContable, " ", _q.NombreCuenta)
                            }).ToList();

            Cls_Datos datos = new Cls_Datos();
            datos.Nombre = "CUENTAS";
            datos.d = qCuentas;

            return datos;
        }


   


        [Route("api/Contabilidad/CatalogoCuenta/Datos")]
        [HttpGet]
        public string Datos()
        {
            return  v_Datos();
        }

        private string v_Datos()
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new List<Cls_Datos>();

                    Cls_Datos datos = v_Obterner_Cuentas();

                    lstDatos.Add(datos);


                    var qGrupos = Conexion.GruposCuentas.ToList();

                    datos = new Cls_Datos();
                    datos.Nombre = "GRUPOS";
                    datos.d = qGrupos;
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


    }
}
