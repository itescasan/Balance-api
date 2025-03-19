using Balance_api.Class;
using Balance_api.Contexts;
using Balance_api.Controllers.Sistema;
using Balance_api.Models.Contabilidad;
using Balance_api.Models.Sistema;
using Balance_api.Reporte.Contabilidad;
using DevExpress.DataAccess.Sql;
using DevExpress.Text.Interop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Nodes;
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


                    var qCentroCosto = Conexion.CatalogoCentroCostos.ToList();


                    datos = new();
                    datos.Nombre = "CENTRO COSTO";
                    datos.d = qCentroCosto;
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


        [Route("api/Contabilidad/AsientoContable/Autorizar")]
        [HttpPost]
        public IActionResult Autorizar(int IdAsiento, string Usuario)
        {
            if (ModelState.IsValid)
            {

                return Ok(V_Autorizar(IdAsiento, Usuario));

            }
            else
            {
                return BadRequest();
            }

        }

        public string V_Autorizar(int IdAsiento, string Usuario)
        {



            string json = string.Empty;

            try
            {
                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                using (Conexion)
                {

                    Asiento A = Conexion.AsientosContables.Find(IdAsiento)!;
                    A.Estado = "AUTORIZADO";
                    A.UsuarioUpdate = Usuario;
                    A.FechaUpdate = DateTime.Now;

                    Conexion.SaveChanges();


                    scope.Complete();


                    Cls_Datos datos = new();
                    datos.Nombre = "AUTORIZAR";
                    datos.d = "Registro Autorizado";
          


                    json = Cls_Mensaje.Tojson(datos, 1, string.Empty, string.Empty, 0);


                    return json;
                }


            }
            catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;

        }




        [Route("api/Contabilidad/AsientoContable/Guardar")]
        [HttpPost]
        public IActionResult Guardar([FromBody] Asiento d)
        {
            if (ModelState.IsValid)
            {

                return Ok(V_GuardarAsiento(d));

            }
            else
            {
                return BadRequest();
            }

        }

        public string V_Guardar(Asiento d, BalanceEntities _Conexion, bool Consecutivo)
        {
            string json = string.Empty;

            bool esNuevo = false;
            decimal TotalDebito = 0;
            decimal TotalCredito = 0;
            Asiento? _Maestro = _Conexion.AsientosContables.Find(d.IdAsiento);
            Periodos? Pi = _Conexion.Periodos.FirstOrDefault(f => f.FechaInicio.Year == d.Fecha.Year && f.FechaInicio.Month == d.Fecha.Month);
            EjercicioFiscal? Ej = _Conexion.EjercicioFiscal.FirstOrDefault(f => f.FechaInicio.Year == d.Fecha.Year);


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


            if (Ej?.Estado == "CERRADO")
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", "El Ejercicio Fiscal se cuentra cerrado", 1);
                return json;
            }


            if (_Maestro == null)
            {

                if(Consecutivo)
                {

                    SerieDocumento sd = _Conexion.SerieDocumento.First(w => w.IdSerie == d.IdSerie);
                    int ConsecutivoSerie = 0;

                    if (sd.TipoDocumento.Automatico)
                    {
                        _Conexion.Database.ExecuteSqlRaw($"UPDATE CNT.SerieDocumentos SET Consecutivo += 1  WHERE  IdSerie = '{d.IdSerie}'");
                        _Conexion.SaveChanges();

                        ConsecutivoSerie = _Conexion.Database.SqlQueryRaw<int>($"SELECT Consecutivo FROM CNT.SerieDocumentos WHERE IdSerie = '{d.IdSerie}'").ToList().First();
                    }
                    else
                    {
                        _Conexion.Database.ExecuteSqlRaw($"UPDATE CNT.ConsecutivoDiario SET Consecutivo += 1  WHERE  IdSerie = '{d.IdSerie}' AND Mes = {d.Fecha.Month}  AND Anio = {d.Fecha.Year}");
                        _Conexion.SaveChanges();

                        ConsecutivoSerie = _Conexion.Database.SqlQueryRaw<int>($"SELECT Consecutivo FROM CNT.ConsecutivoDiario WHERE IdSerie = '{d.IdSerie}' AND Mes = {d.Fecha.Month}  AND Anio = {d.Fecha.Year}").ToList().First();
                    }






                    
                    d.NoAsiento = string.Concat(d.IdSerie, string.Format("{0:yyyyMM}", d.Fecha), "-", ConsecutivoSerie);

                }

             


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
            _Maestro.Estado = d.Estado.ToUpper();
            _Maestro.TipoAsiento = d.TipoAsiento;
            _Maestro.Total = d.Total;
            _Maestro.TotalML = d.TotalML;
            _Maestro.TotalMS = d.TotalMS;
            _Maestro.UsuarioUpdate = d.UsuarioReg;
            _Maestro.FechaUpdate = DateTime.Now;
            if (esNuevo) _Conexion.AsientosContables.Add(_Maestro);

            _Conexion.SaveChanges();

            int x = 1;
            foreach (AsientoDetalle detalle in d.AsientosContablesDetalle.OrderBy(o => o.NoLinea))
            {
                bool esNuevoDet = false;

                AsientoDetalle? _det = _Conexion.AsientosContablesDetalle.FirstOrDefault(w => w.IdDetalleAsiento == detalle.IdDetalleAsiento);

                if (_det == null)
                {
                    esNuevoDet = true;
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
                _det.Naturaleza = detalle.Naturaleza;
                _det.CentroCosto = detalle.CentroCosto;
                _det.NoDocumento = detalle.NoDocumento;
                _det.TipoDocumento = detalle.TipoDocumento;

                if (_det.NoDocumento == string.Empty || _det.NoDocumento == null)
                {
                    _det.NoDocumento = _Maestro.NoAsiento;
                    _det.TipoDocumento = _Maestro.TipoDocOrigen;
                }

                TotalDebito += _det.Debito;
                TotalCredito += _det.Credito;


                if (esNuevoDet) _Conexion.AsientosContablesDetalle.Add(_det);
               

                x++;
            }
            _Conexion.SaveChanges();

            if (TotalDebito - TotalCredito != 0 || TotalDebito == 0 && TotalCredito == 0)
            {
                _Maestro.Estado = "DESCUADRADO";
                _Conexion.SaveChanges();
            }


            List<Cls_Datos> lstDatos = new();



            xrpAsientoContable rpt = new xrpAsientoContable();

            SqlDataSource sqlDataSource = (SqlDataSource)rpt.DataSource;
            sqlDataSource.Connection.ConnectionString = _Conexion.Database.GetConnectionString();


            sqlDataSource.Queries["CNT_RPT_AsientoContable"].Parameters["@P_IdAsiento"].Value = _Maestro.IdAsiento;
            sqlDataSource.Queries["CNT_RPT_AsientoContable"].Parameters["@P_IdMoneda"].Value = _Maestro.IdMoneda;

            MemoryStream stream = new MemoryStream();

            rpt.ExportToPdf(stream, null);
            stream.Seek(0, SeekOrigin.Begin);

            Cls_Datos datos = new();
            datos.d = stream.ToArray();
            datos.Nombre = "REPORTE ASIENTO";
            lstDatos.Add(datos);



             datos = new();
            datos.Nombre = "GUARDAR";
            datos.d = $"<span>Registro Guardado <br> <b style='color:red'>{_Maestro.NoAsiento}</b></span>";
            lstDatos.Add(datos);








            json = Cls_Mensaje.Tojson(lstDatos, lstDatos.Count, string.Empty, string.Empty, 0);


            return json;

        }


        private string V_GuardarAsiento(Asiento d)
        {
            string json = string.Empty;

            try
            {
                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                using (Conexion)
                {

                    json =  V_Guardar(d, Conexion, true);

                    Cls_JSON? reponse = JsonSerializer.Deserialize<Cls_JSON>(json);

                    if (reponse == null) return json;
                    if (reponse.esError == 1) return json;

                    scope.Complete();

                    return json;
                }


            }
            catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;
        }

        [Route("api/Contabilidad/AsientoContable/Get")]
        [HttpGet]
        public string Get(DateTime Fecha1, DateTime Fecha2)
        {
            return V_Get(Fecha1, Fecha2);
        }

        private string V_Get(DateTime Fecha1, DateTime Fecha2)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new();

                    var qAsiento = (from _q in Conexion.AsientosContables
                                    where _q.Fecha.Date >= Fecha1 && _q.Fecha.Date <= Fecha2
                                    orderby  _q.FechaReg descending
                                    select new
                                    {
                                        _q.IdAsiento,
                                        _q.IdPeriodo,
                                        _q.NoAsiento,
                                        _q.IdSerie,
                                        _q.Fecha,
                                        _q.IdMoneda,
                                        _q.TasaCambio,
                                        _q.Concepto,
                                        _q.NoDocOrigen,
                                        _q.IdSerieDocOrigen,
                                        _q.TipoDocOrigen,
                                        _q.Bodega,
                                        _q.Referencia,
                                        _q.Estado,
                                        _q.TipoAsiento,
                                        _q.Total,
                                        _q.TotalML,
                                        _q.TotalMS,
                                        _q.FechaReg,
                                        _q.UsuarioReg,

                                    }).ToList();


                    Cls_Datos datos = new();
                    datos.Nombre = "ASIENTO";
                    datos.d = qAsiento;

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


        [Route("api/Contabilidad/AsientoContable/GetDetalle")]
        [HttpGet]
        public string GetDetalle(int IdAsiento)
        {
            return V_GetDetalle(IdAsiento);
        }

        private string V_GetDetalle(int IdAsiento)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                 

                    Cls_Datos datos = new();
                    datos.Nombre = "ASIENTO";
                    List<AsientoDetalle> lst = Conexion.AsientosContablesDetalle.Where(W => W.IdAsiento == IdAsiento).ToList();
                    lst = lst.Where(w => w.Debito + w.Credito > 0).ToList();
                    datos.d = lst;

                    json = Cls_Mensaje.Tojson(datos, 1, string.Empty, string.Empty, 0);
                }



            }
            catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;
        }



        [Route("api/Contabilidad/AsientoContable/GetReporte")]
        [HttpGet]
        public string DaGetReporte(int IdAsiento, string IdMoneda, bool Exportar, bool Consolidado, bool Unificado)
        {
            return V_GetReporte(IdAsiento, IdMoneda, Exportar, Consolidado, Unificado);
        }

        private string V_GetReporte(int IdAsiento, string IdMoneda, bool Exportar, bool Consolidado, bool Unificado)
        {
            string json = string.Empty;
            if (IdMoneda == null) IdMoneda = string.Empty;
            try
            {
                Cls_Datos Datos = new();

                xrpAsientoContable rpt = new xrpAsientoContable();
                
                SqlDataSource sqlDataSource = (SqlDataSource)rpt.DataSource;

                sqlDataSource.Queries["CNT_RPT_AsientoContable"].Parameters["@P_IdAsiento"].Value = IdAsiento;
                sqlDataSource.Queries["CNT_RPT_AsientoContable"].Parameters["@P_IdMoneda"].Value = IdMoneda;
                sqlDataSource.Queries["CNT_RPT_AsientoContable"].Parameters["@P_Consolidado"].Value = Consolidado;
                sqlDataSource.Queries["CNT_RPT_AsientoContable"].Parameters["@P_Unificado"].Value = Unificado;

                
                MemoryStream stream = new MemoryStream();

                if(Exportar)
                {
                    rpt.ExportToXlsx(stream, null);
                }
                else
                {
                    rpt.ExportToPdf(stream, null);
                    
                }

                stream.Seek(0, SeekOrigin.Begin);


                Datos.d = stream.ToArray();
                Datos.Nombre = "REPORTE ASIENTO";

                json = Cls_Mensaje.Tojson(Datos, 1, string.Empty, string.Empty, 0);




            }
            catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;
        }


    }
}
