
using Balance_api.Class;
using Balance_api.Contexts;
using Balance_api.Models.Contabilidad;
using Balance_api.Models.Proveedor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace Balance_api.Controllers.Proveedor
{
    public class GastosInternosController : Controller
    {

        private readonly BalanceEntities Conexion;

        public GastosInternosController(BalanceEntities db)
        {
            Conexion = db;
        }


        [Route("api/Contabilidad/GastosInternos/Get")]
        [HttpGet]
        public string Get()
        {
            return V_Get();
        }

        private string V_Get()
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {

                    var qQuery =  Conexion.CatalogoGastosInternos.ToList();



                    Cls_Datos datos = new();
                    datos.Nombre = "REGISTROS";
                    datos.d = qQuery;

                    json = Cls_Mensaje.Tojson(datos, qQuery.Count, string.Empty, string.Empty, 0);
                }



            }
            catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;
        }

    





        [Route("api/Proveedor/GastosInternos/Guardar")]
        [HttpPost]
        public IActionResult Guardar([FromBody] CatalogoGastosInternos d)
        {
            if (ModelState.IsValid)
            {

                return Ok(V_Guardar(d));

            }
            else
            {
                return BadRequest();
            }

        }

        private string V_Guardar(CatalogoGastosInternos d)
        {

            string json = string.Empty;

            try
            {

                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                using (Conexion)
                {

                    bool esNuevo = false;
                    int Codigo = 1;
                    CatalogoGastosInternos? _Maestro = Conexion.CatalogoGastosInternos.Find(d.CODIGO);

                    Codigo = Conexion.CatalogoGastosInternos.Max(m => m.CODIGO);




                    if (_Maestro == null)
                    {
                        _Maestro = new CatalogoGastosInternos();
                        _Maestro.CODIGO = Codigo + 1;
                        esNuevo = true;
                    }

                   
                    _Maestro.DESCRIPCION = d.DESCRIPCION;
                    _Maestro.CUENTACONTABLE = d.CUENTACONTABLE;
                    _Maestro.APLICAREN = d.APLICAREN;
                    _Maestro.TIPO = d.TIPO;
                    _Maestro.ESTADO = d.ESTADO;

                    if (esNuevo) Conexion.CatalogoGastosInternos.Add(_Maestro);

                    Conexion.SaveChanges();




         
                    Cls_Datos datos = new();
                    datos.Nombre = "GUARDAR";
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
