using Balance_api.Class;
using Balance_api.Contexts;
using Balance_api.Models.Contabilidad;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace Balance_api.Controllers.Contabilidad
{
    // [Route("api/Contabilidad/[Controller]")]
    public class EjercicioFiscalController : Controller
    {
        private readonly BalanceEntities Conexion;

        public EjercicioFiscalController(BalanceEntities db)
    {
        Conexion = db;
    }

        [Route("api/Contabilidad/EjercicioFiscal/Get")]
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
                    List<Cls_Datos> lstDatos = new();

                    var datos = V_Obterner_Ejercicio();
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

        private Cls_Datos V_Obterner_Ejercicio()
        {

            var qEjercicioFiscal = (from _q in Conexion.EjercicioFiscal
                                    select new
                            {
                                _q.IdEjercicio                             
                            }).ToList();

            Cls_Datos datos = new Cls_Datos();
            datos.Nombre = "EJERCICIO";
            datos.d = qEjercicioFiscal;

            return datos;
        }


        [Route("api/Contabilidad/EjercicioFiscal/Guardar")]
        [HttpPost]
        public IActionResult Guardar([FromBody] EjercicioFiscal d)
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

        private string V_Guardar(EjercicioFiscal d)
        {

            string json = string.Empty;

            try
            {

                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                using (Conexion)
                {

                    bool esNuevo = false;
                    EjercicioFiscal? _Maestro = Conexion.EjercicioFiscal.Find(d.IdEjercicio);


                    if (_Maestro == null)
                    {
                        _Maestro = new EjercicioFiscal();
                        _Maestro.FechaReg = DateTime.Now;
                        _Maestro.IdUsuarioReg = d.IdUsuarioUpdate;
                        esNuevo = true;
                    }



                    _Maestro.IdEjercicio = d.IdEjercicio;
                    _Maestro.Nombre = d.Nombre;

                    _Maestro.FechaUpdate = DateTime.Now;
                    if (esNuevo) Conexion.EjercicioFiscal.Add(_Maestro);

                    Conexion.SaveChanges();




                    List<Cls_Datos> lstDatos = new List<Cls_Datos>();


                    Cls_Datos datos = new Cls_Datos();
                    datos.Nombre = "GUARDAR";
                    datos.d = "Registro Guardado";
                    lstDatos.Add(datos);




                    scope.Complete();

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
