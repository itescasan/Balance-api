using Balance_api.Class;
using Balance_api.Contexts;
using Balance_api.Controllers.Sistema;
using Balance_api.Models.Contabilidad;
using Balance_api.Models.Sistema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace Balance_api.Controllers.Contabilidad
{
    public class AsientoController : Controller
    {
        private readonly BalanceEntities Conexion;

        public AsientoController(BalanceEntities db)
        {
            Conexion = db;
        }


        [Route("api/Contabilidad/AsientoContable/Datos")]
        [HttpGet]
        public string Datos()
        {
            return V_Datos();
        }

        private string V_Datos()
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new();


                    var qBogas = (from _q in Conexion.Bodegas
                                  select new
                                  {
                                      _q.IdBodega,
                                      _q.Codigo,
                                      _q.Bodega
                                  }).ToList();

                    Cls_Datos datos = new();
                    datos.Nombre = "BODEGAS";
                    datos.d = qBogas;

                    lstDatos.Add(datos);


                    var qCuentas = (from _q in Conexion.CatalogoCuenta
                                    where _q.ClaseCuenta == "D"
                                    select new
                                    {
                                        _q.CuentaContable,
                                        NombreCuenta =  string.Concat(_q.CuentaContable, " ", _q.NombreCuenta),
                                        _q.ClaseCuenta,
                                        _q.Naturaleza,
                                        _q.Bloqueada
                                    }).ToList();

                    datos = new();
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




        [Route("api/Contabilidad/Asiento/Guardar")]
        [HttpPost]
        public IActionResult Guardar([FromBody] Asiento d)
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

        private string V_Guardar(Asiento d)
        {

            string json = string.Empty;

            try
            {

                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                using (Conexion)
                {

                    bool esNuevo = false;
                    Asiento? _Maestro = Conexion.AsientosContables.Find(d.IdAsiento);
                    Periodos? Pi = Conexion.Periodos.FirstOrDefault(f => f.FechaInicio.Year == d.Fecha.Year);
                    EjercicioFiscal? Ej = Conexion.EjercicioFiscal.FirstOrDefault(f => f.FechaInicio.Year == d.Fecha.Year);


                    if (Pi == null)
                    {
                        json = Cls_Mensaje.Tojson(null, 0, "1", "No se ha configurado el periodo", 1);
                        return json;
                    }

                    if (Pi.Estado == "BLOQUEADO")
                    {
                        json = Cls_Mensaje.Tojson(null, 0, "1", "El periodo se encuentra bloqueado.", 1);
                        return json;
                    }


                    if (Ej.Estado == "CERRADO")
                    {
                        json = Cls_Mensaje.Tojson(null, 0, "1", "El Ejercicio Fiscal se cuentra cerrado", 1);
                        return json;
                    }


                    if (_Maestro == null)
                    {

                        Conexion.Database.ExecuteSql($"UPDATE CNT.SerieDocumento SET Consecutivo += 1  WHERE  IdSerie = '{d.IdSerie}'");
                        Conexion.SaveChanges();

                        int ConsecutivoSerie = Conexion.Database.SqlQuery<int>($"SELECT Consecutivo FROM CNT.SerieDocumento WHERE IdSerie = '{d.IdSerie}'").First();

                        SerieDocumento? s = Conexion.SerieDocumento.Find(d.IdSerie);
                        s!.Consecutivo = ConsecutivoSerie;
                        d.NoAsiento = string.Concat(d.IdSerie, s!.Consecutivo);


                        _Maestro = new Asiento();
                        _Maestro.FechaReg = DateTime.Now;
                        _Maestro.UsuarioReg = d.UsuarioReg;
                        esNuevo = true;
                    }

                    d.IdPeriodo = Pi.IdPeriodo;

                    _Maestro.IdPeriodo = d.IdPeriodo;
                    _Maestro.NoAsiento = d.NoAsiento;
                    _Maestro.IdSerie = d.IdSerie;
                    _Maestro.Fecha = d.Fecha;
                    _Maestro.IdMoneda = d.IdMoneda;
                    _Maestro.TasaCambio = d.TasaCambio;
                    _Maestro.Concepto = d.Concepto;
                    _Maestro.NoDocOrigen = d.NoDocOrigen;
                    _Maestro.IdSerieDocOrigen = d.IdSerieDocOrigen;
                    _Maestro.TipoDocOrigen = d.TipoDocOrigen;
                    _Maestro.Bodega = d.Bodega;
                    _Maestro.Referencia = d.Referencia;
                    _Maestro.Estado = d.Estado;
                    _Maestro.TipoAsiento = d.TipoAsiento;
                    _Maestro.Total = d.Total;
                    _Maestro.TotalML = d.TotalML;
                    _Maestro.TotalMS = d.TotalMS;
                    _Maestro.UsuarioUpdate = d.UsuarioReg;
                    _Maestro.FechaUpdate = DateTime.Now;
                    if (esNuevo) Conexion.AsientosContables.Add(_Maestro);

                    Conexion.SaveChanges();

                    int x = 1;
                    foreach(AsientoDetalle detalle in d.AsientosContablesDetalle.OrderBy(o => o.NoLinea))
                    {
                        bool esNuevoDet = false;

                        AsientoDetalle? _det = Conexion.AsientosContablesDetalle.Find(detalle.IdDetalleAsiento);

                        if(_det == null)
                        {
                            esNuevoDet = false;
                            _det = new AsientoDetalle();
                        }

                        _det.IdAsiento = _Maestro.IdAsiento;
                        _det.NoLinea = x;
                        _det.CuentaContable = detalle.CuentaContable;
                        _det.Debito = detalle.Debito;
                        _det.DebitoML = detalle.DebitoML;
                        _det.DebitoMS = detalle.DebitoMS;
                        _det.Credito = detalle.Credito;
                        _det.CreditoML = detalle.CreditoML;
                        _det.CreditoMS = detalle.CreditoMS;
                        _det.Modulo = detalle.Modulo;
                        _det.Descripcion = detalle.Descripcion;
                        _det.Referencia = detalle.Referencia;


                        if (esNuevoDet) Conexion.AsientosContablesDetalle.Add(_det);

                        x++;
                    }
                    Conexion.SaveChanges();



                    List<Cls_Datos> lstDatos = new();


                    Cls_Datos datos = new();
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
