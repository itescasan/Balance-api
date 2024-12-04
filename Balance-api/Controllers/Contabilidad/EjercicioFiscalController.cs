using Azure;
using Balance_api.Class;
using Balance_api.Contexts;
using Balance_api.Models.Contabilidad;
using Balance_api.Models.Sistema;
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
                                _q.IdEjercicio,
                                _q.Nombre,
                                        FechaInicio = string.Format("{0:yyyy-MM-dd}", _q.FechaInicio),
                                        FechaFinal = string.Format("{0:yyyy-MM-dd}", _q.FechaFinal),
                                        _q.ClasePeriodos,
                                _q.NumerosPeriodos,
                                _q.Estado,
                                _q.CuentaContableAcumulada,
                                _q.CuentaPerdidaGanancia,
                                _q.CuentaContablePeriodo,
                                _q.FechaReg,
                                _q.UsuarioReg,
                                _q.FechaUpdate,
                                _q.UsuarioUpdate,
                                _q.Periodos
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
                    bool EsNuevoDet = false;
                    EjercicioFiscal? _Maestro = Conexion.EjercicioFiscal.Find(d.IdEjercicio);


                    EjercicioFiscal? _FEchaEj = Conexion.EjercicioFiscal.FirstOrDefault(f=>f.FechaInicio.Year == d.FechaInicio.Year );

                   
                    if (_Maestro == null)
                    {
                        _Maestro = new EjercicioFiscal();
                        _Maestro.FechaReg = DateTime.Now;
                        _Maestro.UsuarioReg = d.UsuarioReg;
                        esNuevo = true;

                        if (_FEchaEj != null)
                        {
                            json = Cls_Mensaje.Tojson(null, 0, "1", "Ya se encuentra Registrada este año para este Ejercicio Fiscal.", 1);
                            return json;
                        }
                    }



                    _Maestro.IdEjercicio = d.IdEjercicio;                    
                    _Maestro.Nombre = d.Nombre;
                    _Maestro.FechaInicio = d.FechaInicio;
                    _Maestro.FechaFinal = d.FechaFinal;
                    _Maestro.ClasePeriodos = d.ClasePeriodos;
                    _Maestro.NumerosPeriodos = d.NumerosPeriodos;
                    _Maestro.Estado = d.Estado; 
                    _Maestro.CuentaContableAcumulada = d.CuentaContableAcumulada;
                    _Maestro.CuentaPerdidaGanancia = d.CuentaPerdidaGanancia;
                    _Maestro.CuentaContablePeriodo = d.CuentaContablePeriodo;
                    _Maestro.FechaReg = d.FechaReg;
                    _Maestro.UsuarioReg = d.UsuarioReg;

                    _Maestro.FechaUpdate = DateTime.Now;
                    _Maestro.UsuarioUpdate = d.UsuarioReg;

                    if (esNuevo) Conexion.EjercicioFiscal.Add(_Maestro);

                    Conexion.SaveChanges();

                    foreach (var det in d.Periodos)
                    {
                        Periodos? _Detalle = Conexion.Periodos.Find(det.IdPeriodo);

                        EsNuevoDet = false;


                        if (_Detalle == null)
                        {
                            _Detalle = new Periodos();
                            _Detalle.IdPeriodo = det.IdPeriodo;
                            _Detalle.FechaReg = DateTime.Now;
                            _Detalle.UsuarioReg = det.UsuarioReg;
                            _Detalle.IdEjercicio = _Maestro.IdEjercicio;
                            EsNuevoDet = true;
                        }                       
                        
                        
                        _Detalle.NoPeriodo = det.NoPeriodo;
                        _Detalle.NombrePeriodo = det.NombrePeriodo;
                        _Detalle.ClasePeriodo = det.ClasePeriodo;
                        _Detalle.FechaInicio = det.FechaInicio;
                        _Detalle.FechaFinal = det.FechaFinal;
                        _Detalle.Estado = det.Estado;                        
                        _Detalle.UsuarioUpdate = det.UsuarioReg;
                        _Detalle.FechaUpdate = DateTime.Now;
                    
                        
                        if (EsNuevoDet) Conexion.Periodos.Add(_Detalle);
                        Conexion.SaveChanges();

                    }
                    


                    List<Cls_Datos> lstDatos = new();


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



        [Route("api/Contabilidad/EjercicioFiscal/Periodo")]
        [HttpGet]
        public string GetP(int IdEjercicio)
        {
            return V_GetPeriodo(IdEjercicio);
        }

        private string V_GetPeriodo(int IdEjercicio)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new();

                    var datos = V_Obterner_Periodo (IdEjercicio);
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

       

        private Cls_Datos V_Obterner_Periodo(int IdEjercicio)
        {

            var qPeriodos = (from _q in Conexion.Periodos where _q.IdEjercicio == IdEjercicio
                             select new
                                    {
                                     _q.IdPeriodo,
                                     _q.IdEjercicio,
                                     _q.NoPeriodo,
                                     _q.NombrePeriodo,
                                     _q.ClasePeriodo,
                                     _q.FechaInicio,
                                     _q.FechaFinal,
                                     _q.Estado,
                                     _q.FechaReg,
                                     _q.UsuarioReg,
                                     _q.FechaUpdate,
                                     _q.UsuarioUpdate                                     
                                    }).ToList();

            Cls_Datos datos = new Cls_Datos();
            datos.Nombre = "PERIODOS";
            datos.d = qPeriodos;

            return datos;
        }


    }
}
