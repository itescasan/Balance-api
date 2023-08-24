using Balance_api.Class;
using Balance_api.Contexts;
using Microsoft.AspNetCore.Mvc;

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
        public string Get()
        {
            return v_Get();
        }

        private string v_Get()
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new List<Cls_Datos>();


                    var qCuentas = (from _q in Conexion.CatalogoCuenta
                                    select new
                                    {
                                        _q.CuentaContable,
                                        _q.NombreCuenta,
                                        _q.Nivel,
                                        _q.GrupoCuentas,
                                        _q.ClaseCuenta,
                                        _q.CuentaPadre,
                                        _q.Naturaleza,
                                        _q.Bloqueada,
                                        Filtro = string.Concat(_q.CuentaContable, _q.NombreCuenta)
                                    }).ToList();


                    Cls_Datos datos = new Cls_Datos();
                    datos.Nombre = "CUENTAS";
                    datos.d = qCuentas;
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
