using Balance_api.Class;
using Balance_api.Class.Contabilidad;
using Balance_api.Contexts;
using Balance_api.Controllers.Sistema;
using Balance_api.Models.Contabilidad;
using Balance_api.Models.Inventario;
using Balance_api.Models.Proveedor;
using Balance_api.Models.Sistema;
using DevExpress.Charts.Native;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.Json;
using System.Transactions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Balance_api.Controllers.Contabilidad
{
    public class TransferenciaController : Controller
    {
        private readonly BalanceEntities Conexion;

        public TransferenciaController(BalanceEntities db)
        {
            Conexion = db;
        }


        [Route("api/Contabilidad/Transferencia/Datos")]
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



                    var qCuentaBancaria = (from _q in Conexion.CuentaBanco
                                           where _q.Activo && _q.Tipo == "T"
                                           orderby _q.IdBanco
                                  select new
                                  {
                                      _q.IdCuentaBanco,
                                      _q.Bancos.Banco,
                                      _q.CuentaBancaria,
                                      _q.NombreCuenta,
                                      _q.IdMoneda,
                                      _q.Monedas.Moneda,
                                      _q.Bancos.CuentaNuevaC,
                                      _q.Bancos.CuentaNuevaD,
                                      IdSerie = "EG",
                                      Consecutivo = string.Concat(_q.IdSerie, _q.SerieDocumento.Consecutivo + 1),
                                      _q.Activo,
                                      DisplayKey = string.Concat(_q.Bancos.Banco, " ", _q.NombreCuenta, " ", _q.Monedas.Simbolo, " ", _q.CuentaBancaria),
                                  }).ToList();

                    Cls_Datos datos = new();
                    datos.Nombre = "CUENTA BANCARIA";
                    datos.d = qCuentaBancaria;

                    lstDatos.Add(datos);



                    var qBogas = (from _q in Conexion.Bodegas
                                  select new
                                  {
                                      _q.IdBodega,
                                      _q.Codigo,
                                      _q.Bodega
                                  }).ToList();

                    datos = new();
                    datos.Nombre = "BODEGAS";
                    datos.d = qBogas;

                    lstDatos.Add(datos);


                    var qCuentas = (from _q in Conexion.CatalogoCuenta
                                    where _q.ClaseCuenta == "D"
                                    select new
                                    {
                                        _q.CuentaContable,
                                        NombreCuenta = string.Concat(_q.CuentaContable, " ", _q.NombreCuenta),
                                        _q.ClaseCuenta,
                                        _q.Naturaleza,
                                        _q.Bloqueada
                                    }).ToList();

                    datos = new();
                    datos.Nombre = "CUENTAS";
                    datos.d = qCuentas;
                    lstDatos.Add(datos);




                    var qCentroCosto = Conexion.CentroCostos.ToList();


                    datos = new();
                    datos.Nombre = "CENTRO COSTO";
                    datos.d = qCentroCosto;
                    lstDatos.Add(datos);


                    var qProveedor = (from _q in Conexion.Proveedor
                                      select new
                                      {
                                          _q.IdProveedor,
                                          _q.Codigo,
                                          Proveedor = _q.Proveedor1,
                                          _q.NombreComercial,
                                          _q.CUENTAXPAGAR,
                                          DisplayKey = string.Concat(_q.Codigo, " ", _q.Proveedor1),
                                      }).ToList();


                    datos = new();
                    datos.Nombre = "PROVEEDOR";
                    datos.d = qProveedor;
                    lstDatos.Add(datos);


                    var qReembolsos = (from _q in Conexion.IngresoC
                                       join _c in Conexion.CatalogoCuenta on _q.Cuenta equals _c.CuentaContable
                                       where _q.Aplicado == true && _q.Contabilizado == false
                                       orderby _q.Consecutivo
                                       select new
                                       {
                                           _q.IdIngresoCajaChica,
                                           _q.Cuenta,
                                           key = string.Concat(_q.Consecutivo, " ", _c.NombreCuenta),
                                           NombreCuenta = string.Concat(_q.Consecutivo, " ", _c.NombreCuenta),
                                           DetalleCaja = _q.DetalleCaja.ToList(),
                                       }).ToList();



                    datos = new();
                    datos.Nombre = "Reembolso";
                    datos.d = qReembolsos;
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





        [Route("api/Contabilidad/Transferencia/GetDocumentos")]
        [HttpGet]
        public string GetDocumentos(string CodProveedor)
        {
            return V_GetDocumentos(CodProveedor);
        }

        private string V_GetDocumentos(string CodProveedor)
        {
            string json = string.Empty;
            try
            {
        
                using (Conexion)
                {
                    var qDoc = Conexion.MovimientoDoc.Where(w => w.CodigoCliente == CodProveedor && w.Activo && w.Esquema == "CXP").ToList();

                    List<TransferenciaDocumento> qDocumentos = (from _q in qDoc
                                       where _q.Activo
                                       group _q by new
                                       {
                                           NoDococumento = (_q.NoDocEnlace == null ? _q.NoDocOrigen : _q.NoDocEnlace),
                                           Serie = (_q.NoDocEnlace == null ? _q.SerieOrigen : _q.SerieEnlace),
                                           TipoDocumento = (_q.NoDocEnlace == null ? _q.TipoDocumentoOrigen : _q.TipoDocumentoEnlace),
                                       } into grupo
                                       select new TransferenciaDocumento
                                       {
                                           IdDetTrasnfDoc = new Guid(),
                                           Index = 0,
                                           Documento = grupo.Key.NoDococumento,
                                           Serie = grupo.Key.Serie,
                                           TipoDocumento = grupo.Key.TipoDocumento,
                                           Fecha = (DateTime)qDoc.FirstOrDefault(f => f.NoDocOrigen == grupo.Key.NoDococumento)?.FechaDocumento.Date!,
                                           IdMoneda = qDoc.FirstOrDefault(f => f.NoDocOrigen == grupo.Key.NoDococumento)?.IdMoneda!,
                                           TasaCambioDoc = (decimal)qDoc.FirstOrDefault(f => f.NoDocOrigen == grupo.Key.NoDococumento)?.TasaCambio!,
                                           SaldoCordoba = grupo.Sum(s => s.TotalCordoba),
                                           SaldoDolar = grupo.Sum(s => s.TotalDolar)
                                       }).ToList();


                    qDocumentos = qDocumentos.Where(w => w.SaldoCordoba > 0).ToList();

                 

                    var Doc = qDocumentos.Select((file, index) => new { Index = index,  file.Documento,  file.Serie,file.TipoDocumento,
                         file.Fecha,  file.IdMoneda, file.TasaCambioDoc,  file.SaldoDolar,  file.SaldoCordoba
                    }).ToList();

                    Cls_Datos  datos = new();
                    datos.Nombre = "DOC PROVEEDOR";
                    datos.d = Doc;
               


                    json = Cls_Mensaje.Tojson(datos, 1, string.Empty, string.Empty, 0);
                }



            }
            catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;
        }




        [Route("api/Contabilidad/Transferencia/Guardar")]
        [HttpPost]
        public IActionResult Guardar([FromBody] Cls_Datos_TransferenciaCuenta d)
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

        private string V_Guardar(Cls_Datos_TransferenciaCuenta d)
        {

            string json = string.Empty;

            try
            {

                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                using (Conexion)
                {

                    bool esNuevo = false;
                    Transferencia? _Transf = Conexion.Transferencia.Find(d.T.IdTransferencia);
                    string IdMonedaLocal =  Conexion.Database.SqlQueryRaw<string>($"SELECT TOP 1 MonedaLocal FROM SIS.Parametros").ToList().First();

                    if (_Transf == null)
                    {

                        Conexion.Database.ExecuteSqlRaw($"UPDATE CNT.ConsecutivoDiario SET Consecutivo += 1  WHERE  IdSerie = '{d.T.IdSerie}' AND Mes = {d.T.Fecha.Month}  AND Anio = {d.T.Fecha.Year}");
                        Conexion.SaveChanges();

                        int ConsecutivoSerie = Conexion.Database.SqlQueryRaw<int>($"SELECT Consecutivo FROM CNT.ConsecutivoDiario WHERE IdSerie = '{d.T.IdSerie}' AND Mes = {d.T.Fecha.Month}  AND Anio = {d.T.Fecha.Year}").ToList().First();

                        d.T.NoTransferencia = string.Concat(d.T.IdSerie, string.Format("{0:yyyyMM}", d.T.Fecha), "-", ConsecutivoSerie);


                        d.A.TipoDocOrigen = d.T.TipoTransferencia == "C" ? "TRANSFERENCIA A CUENTA" : "TRANSFERENCIA A DOCUMENTO";
                        d.A.NoDocOrigen = d.T.NoTransferencia;
                        d.A.NoAsiento = d.T.NoTransferencia;
                        d.A.IdSerie = d.T.IdSerie;
                        d.A.IdSerieDocOrigen = d.A.IdSerie;
                        d.A.Estado = "Autorizado";

                        _Transf = new Transferencia();
                        _Transf.IdTransferencia = Guid.NewGuid();
                        d.T.IdTransferencia = _Transf.IdTransferencia;
                        _Transf.FechaReg = DateTime.Now;
                        _Transf.UsuarioReg = d.T.UsuarioReg;
                        _Transf.Anulado = false;
                        esNuevo = true;
                    }

                    _Transf.IdCuentaBanco = d.T.IdCuentaBanco;
                    _Transf.IdMoneda = d.T.IdMoneda;
                    _Transf.CodBodega = d.T.CodBodega;
                    _Transf.IdSerie = d.T.IdSerie;
                    _Transf.NoTransferencia = d.T.NoTransferencia;
                    _Transf.Fecha = d.T.Fecha;
                    _Transf.Beneficiario = d.T.Beneficiario;
                    _Transf.CodProveedor = d.T.CodProveedor;
                    _Transf.TasaCambio = d.T.TasaCambio;
                    _Transf.Concepto = d.T.Concepto;
                    _Transf.TipoTransferencia = d.T.TipoTransferencia;
                    _Transf.CodBodega = d.T.CodBodega;
                    _Transf.Comision = d.T.Comision;
                    _Transf.ComisionDolar = d.T.ComisionDolar;
                    _Transf.ComisionCordoba = d.T.ComisionCordoba;
                    _Transf.Total = d.T.Total;
                    _Transf.TotalDolar = d.T.TotalDolar;
                    _Transf.TotalCordoba = d.T.TotalCordoba;
                    _Transf.IdIngresoCajaChica = d.T.IdIngresoCajaChica;
                    _Transf.CuentaCaja = d.T.CuentaCaja;


                    _Transf.UsuarioUpdate = d.T.UsuarioReg;
                    _Transf.FechaUpdate = DateTime.Now;
                    if (esNuevo) Conexion.Transferencia.Add(_Transf);

                    Conexion.SaveChanges();

                    if (_Transf.IdIngresoCajaChica > 0)
                    {
                        IngresoCaja? det = Conexion.IngresoC.FirstOrDefault(f => f.IdIngresoCajaChica == _Transf.IdIngresoCajaChica);

                        if (det != null)
                        {
                            //Conexion.DetIngCaja.Remove(det!);
                            det.Aplicado = true;
                            det.UsuarioModifica = _Transf.UsuarioReg;
                            det.FechaModificacion = DateTime.Now;
                            Conexion.SaveChanges();
                            Conexion.Database.ExecuteSqlRaw($"UPDATE CNT.IngresosCajaChica SET Contabilizado = 1  WHERE  IdIngresoCajaChica = '{_Transf.IdIngresoCajaChica}'");
                        }

                    }

                    if (d.T.TransferenciaDocumento != null)
                    {

        


                        int i = 0;
                        foreach (TransferenciaDocumento doc in d.T.TransferenciaDocumento)
                        {
                            bool esNuevoDet = false;

                            TransferenciaDocumento? det = Conexion.TransferenciaDocumento.Find(doc.IdDetTrasnfDoc);

                            if (det == null)
                            {
                                esNuevoDet = true;
                                det = new TransferenciaDocumento();
                                det.IdDetTrasnfDoc = Guid.NewGuid();
                            }

                            det.IdTransferencia = _Transf.IdTransferencia;
                            det.Index = i;
                            det.Operacion = doc.Operacion;
                            det.Documento = doc.Documento;
                            det.Serie = doc.Serie;
                            det.TipoDocumento = doc.TipoDocumento;
                            det.Fecha = doc.Fecha;
                            det.IdMoneda = doc.IdMoneda;
                            det.TasaCambioDoc = doc.TasaCambioDoc;
                            det.SaldoAnt = doc.SaldoAnt;
                            det.SaldoAntML = doc.SaldoAntML;
                            det.SaldoAntMS = doc.SaldoAntMS;
                            det.Saldo = doc.Saldo;
                            det.SaldoDolar = doc.SaldoDolar;
                            det.SaldoCordoba = doc.SaldoCordoba;
                            det.Importe = doc.Importe;
                            det.ImporteML = doc.ImporteML;
                            det.ImporteMS = doc.ImporteMS;
                            det.NuevoSaldo = doc.NuevoSaldo;
                            det.NuevoSaldoML = doc.NuevoSaldoML;
                            det.NuevoSaldoMS = doc.NuevoSaldoMS;
                            det.DiferencialML = doc.DiferencialML;
                            det.DiferencialMS = doc.DiferencialMS;
                            det.Retenido = doc.Retenido;


                            if (esNuevoDet) Conexion.TransferenciaDocumento.Add(det);


                            i++;



                            MovimientoDoc? mdoc = Conexion.MovimientoDoc.FirstOrDefault(ff => ff.NoDocOrigen == _Transf.NoTransferencia && ff.SerieOrigen == _Transf.IdSerie && ff.TipoDocumentoOrigen == "TRANSF" && ff.NoDocEnlace == det.Documento && ff.TipoDocumentoEnlace == det.TipoDocumento && ff.Esquema == "CXP");     
                            Bodegas? bo = Conexion.Bodegas.FirstOrDefault(ff => ff.Codigo == _Transf.CodBodega);

                            if (mdoc != null)
                            {
                                Conexion.MovimientoDoc.Remove(mdoc);
                                Conexion.SaveChanges();
                            }

                            mdoc = new MovimientoDoc();
                            mdoc.NoMovimiento = string.Empty;
                            mdoc.IdBodega = (bo == null? 0 : bo.IdBodega);
                            mdoc.CodigoBodega = (bo == null ? string.Empty : bo.Codigo);
                            mdoc.CodigoCliente = _Transf.CodProveedor!;
                            mdoc.CodVendedor = string.Empty;
                            mdoc.NoDocOrigen = _Transf.NoTransferencia;
                            mdoc.SerieOrigen = _Transf.IdSerie;
                            mdoc.FechaDocumento = _Transf.Fecha;
                            mdoc.Plazo = 30;
                            mdoc.DiaGracia = 0;
                            mdoc.TipoVenta = "Credito";
                            mdoc.TasaCambio = _Transf.TasaCambio;
                            mdoc.TipoDocumentoOrigen = "TRANSF";
                            mdoc.IdMoneda = _Transf.IdMoneda;
                            mdoc.NoDocEnlace = det.Documento;
                            mdoc.SerieEnlace = det.Serie;
                            mdoc.TipoDocumentoEnlace = det.TipoDocumento;
                            mdoc.SubTotal = 0;
                            mdoc.SubTotalDolar = 0;
                            mdoc.SubTotalCordoba = 0;
                            mdoc.Descuento = 0;
                            mdoc.DescuentoDolar = 0;
                            mdoc.DescuentoCordoba = 0;
                            mdoc.SubTotalNeto = 0;
                            mdoc.SubTotalNetoDolar = 0;
                            mdoc.SubTotalNetoCordoba = 0;
                            mdoc.Impuesto = 0;
                            mdoc.ImpuestoDolar = 0;
                            mdoc.ImpuestoCordoba = 0;
                            mdoc.Total = (det.IdMoneda == IdMonedaLocal ? det.ImporteML : det.ImporteMS) * -1;
                            mdoc.TotalDolar = ((det.ImporteMS + det.DiferencialMS) * -1);
                            mdoc.TotalCordoba = ((det.ImporteML + det.DiferencialML) * -1);
                            mdoc.DiferencialDolar = det.DiferencialMS;
                            mdoc.DiferencialCordoba = det.DiferencialML;
                            mdoc.RetenidoAlma = 0;
                            mdoc.RetenidoAlmaDolar = 0;
                            mdoc.RetenidoAlmaCordoba = 0;
                            mdoc.RetenidoIR = 0;
                            mdoc.RetenidoIRDolar = 0;
                            mdoc.RetenidoIRCordoba = 0;
                            mdoc.Operacion = det.Operacion;
                            mdoc.Activo = true;
                            mdoc.Esquema = "CXP";
                            Conexion.MovimientoDoc.Add(mdoc);
                            Conexion.SaveChanges();

                            int num = 0;
                            int.TryParse(Conexion.MovimientoDoc.Where(fF => fF.Esquema == "CXP" && fF.NoMovimiento != string.Empty).Max(m => m.NoMovimiento), out num);
                            num++;

                            mdoc.NoMovimiento = num.ToString();
                            Conexion.SaveChanges();


                        }


                        i = 0;

                        foreach(TranferenciaRetencion doc in d.T.TranferenciaRetencion)
                        {
                            bool esNuevoDet = false;

                            TranferenciaRetencion? ret = Conexion.TranferenciaRetencion.Find(doc.IdDetRetencion);

                            if (ret == null)
                            {
                                esNuevoDet = true;
                                ret = new TranferenciaRetencion();
                                ret.IdDetRetencion = Guid.NewGuid();
                            }

                            ret.IdTransferencia = _Transf.IdTransferencia;
                            ret.Index = i;
                            ret.Retencion = doc.Retencion;
                            ret.Porcentaje = doc.Porcentaje;
                            ret.Documento = doc.Documento;
                            ret.Serie = doc.Serie;
                            ret.TipoDocumento = doc.TipoDocumento;
                            ret.IdMoneda = doc.IdMoneda;
                            ret.TasaCambio = doc.TasaCambio;
                            ret.Monto = doc.Monto;
                            ret.MontoMS = doc.MontoMS;
                            ret.MontoML = doc.MontoML;
                            ret.PorcImpuesto = doc.PorcImpuesto;
                            ret.TieneImpuesto = doc.TieneImpuesto;
                            ret.CuentaContable = doc.CuentaContable;


                            if (esNuevoDet) Conexion.TranferenciaRetencion.Add(ret);


                            i++;

                        }

                    
                    }






                    Asiento? _Asiento = null;

                    if (esNuevo)
                    {
                        _Asiento = d.A;
                    }
                    else
                    {
                        _Asiento = Conexion.AsientosContables.FirstOrDefault(f => f.NoDocOrigen == _Transf.NoTransferencia && f.IdSerieDocOrigen == d.T.IdSerie && f.TipoDocOrigen == (d.T.TipoTransferencia == "C" ? "TRANSFERENCIA A CUENTA" : "TRANSFERENCIA A DOCUMENTO"));

                        _Asiento!.AsientosContablesDetalle = d.A.AsientosContablesDetalle;

                    }

                    AsientoController _Controller = new(Conexion);
                    json = _Controller.V_Guardar(_Asiento!, Conexion, false);

                    Cls_JSON? reponse = JsonSerializer.Deserialize<Cls_JSON>(json);

                    if (reponse == null) return json;
                    if (reponse.esError == 1) return json;


                    List<Cls_Datos> lstDatos = new();


                    Cls_Datos datos = new();
                    datos.Nombre = "GUARDAR";
                    datos.d = $"<span>Registro Guardado <br> <b style='color:red'>{_Transf.NoTransferencia}</b></span>";
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




        [Route("api/Contabilidad/Transferencia/Get")]
        [HttpGet]
        public string Get(DateTime Fecha1, DateTime Fecha2, string CodBodega)
        {
            return V_Get(Fecha1, Fecha2, CodBodega);
        }

        private string V_Get(DateTime Fecha1, DateTime Fecha2, string CodBodega)
        {
            CodBodega ??= string.Empty;

            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new();


                    var qTransferencia = (from _q in Conexion.Transferencia
                                          join _p in Conexion.Proveedor on _q.CodProveedor equals _p.Codigo into _q_p
                                          from u in _q_p.DefaultIfEmpty()
                                          where _q.CodBodega == (CodBodega == string.Empty ? _q.CodBodega : CodBodega) && _q.Fecha.Date >= Fecha1.Date && _q.Fecha <= Fecha2.Date
                                          orderby _q.NoTransferencia descending
                                          select new
                                          {
                                              _q.IdTransferencia,
                                              _q.IdCuentaBanco,
                                              CuentaBancaria = string.Concat(_q.CuentaBanco.Bancos.Banco, " ", _q.CuentaBanco.NombreCuenta, " ", _q.CuentaBanco.Monedas.Simbolo, " ", _q.CuentaBanco.CuentaBancaria),
                                              _q.IdMoneda,
                                              _q.CodBodega,
                                              _q.IdSerie,
                                              _q.NoTransferencia,
                                              _q.Fecha,
                                              Beneficiario = _q.TipoTransferencia == "C"? _q.Beneficiario : u.Proveedor1,
                                              _q.CodProveedor,
                                              _q.TasaCambio,
                                              _q.Concepto,
                                              _q.TipoTransferencia,
                                              _q.IdIngresoCajaChica,
                                              _q.CuentaCaja,
                                              _q.Total,
                                              _q.TotalDolar,
                                              _q.TotalCordoba,
                                              _q.Comision,
                                              _q.ComisionCordoba,
                                              _q.ComisionDolar,
                                              _q.Anulado,
                                              _q.UsuarioReg,
                                              _q.FechaReg
                                              
                                          }).ToList();




                    Cls_Datos datos = new();
                    datos.Nombre = "TRANSFERENCIA";
                    datos.d = qTransferencia;

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


        [Route("api/Contabilidad/Transferencia/GetDetalleCuenta")]
        [HttpGet]
        public string GetDetalleCuenta(Guid IdTransferencia)
        {
            return V_GetDetalleCuenta(IdTransferencia);
        }

        private string V_GetDetalleCuenta(Guid IdTransferencia)
        {
     
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new();

                    Transferencia T = Conexion.Transferencia.Find(IdTransferencia)!;

                    var A = (from _q in Conexion.AsientosContables
                                 where _q.NoDocOrigen == T.NoTransferencia && _q.IdSerieDocOrigen == T.IdSerie && _q.TipoDocOrigen == "TRANSFERENCIA A CUENTA"
                                 select _q.AsientosContablesDetalle).ToList();


  

                    Cls_Datos datos = new();
                    datos.Nombre = "DETALLE TRANSFERENCIA";
                    datos.d = A.First();

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


        [Route("api/Contabilidad/Transferencia/GetDetalleDocumentos")]
        [HttpGet]
        public string GetDetalleDocumentos(Guid IdTransferencia)
        {
            return V_GetDetalleDocumentos(IdTransferencia);
        }

        private string V_GetDetalleDocumentos(Guid IdTransferencia)
        {

            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new();

                    Transferencia T = Conexion.Transferencia.Find(IdTransferencia)!;



                    var qDocumentos = (from _q in Conexion.Transferencia
                                       where _q.IdTransferencia == IdTransferencia
                                       select _q.TransferenciaDocumento).First();





                    Cls_Datos datos = new();
                    datos.Nombre = "DETALLE DOCUMENTOS";
                    datos.d = qDocumentos;
                    lstDatos.Add(datos);



                    var qRetenciones = (from _q in Conexion.Transferencia
                                       where _q.IdTransferencia == IdTransferencia
                                       select _q.TranferenciaRetencion).First();



                    datos = new();
                    datos.Nombre = "RETENCIONES";
                    datos.d = qRetenciones;

                    lstDatos.Add(datos);



                    var A = (from _q in Conexion.AsientosContables
                             where _q.NoDocOrigen == T.NoTransferencia && _q.IdSerieDocOrigen == T.IdSerie && _q.TipoDocOrigen == "TRANSFERENCIA A DOCUMENTO"
                             select new { 
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
                                 _q.UsuarioReg

                             }).ToList();




                    datos = new();
                    datos.Nombre = "ASIENTO";
                    datos.d = A.First();

                    lstDatos.Add(datos);


                    var D = (from _q in Conexion.AsientosContables
                             where _q.NoDocOrigen == T.NoTransferencia && _q.IdSerieDocOrigen == T.IdSerie && _q.TipoDocOrigen == "TRANSFERENCIA A DOCUMENTO"
                             select _q.AsientosContablesDetalle).ToList();




                    datos = new();
                    datos.Nombre = "DETALLE ASIENTO";
                    datos.d = D.First();

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





        [Route("api/Contabilidad/Transferencia/BuscarTiposRetenciones")]
        [HttpGet]
        public string BuscarTiposRetenciones(string NoDocumento, string TipoDocumento)
        {
            return V_BuscarTiposRetenciones(NoDocumento, TipoDocumento);
        }

        private string V_BuscarTiposRetenciones(string NoDocumento, string TipoDocumento)
        {

            string json = string.Empty;
            try
            {
                using (Conexion)
                {

                    List<Cls_Datos> lstDatos = new();

                    List<Retenciones> R = Conexion.Retenciones.Where(w => w.AplicaEnCXC == false).ToList();
          
                    Cls_Datos datos = new();
                    datos = new();
                    datos.Nombre = "RETENCIONES";
                    datos.d = R;
                    lstDatos.Add(datos);

                    MovimientoDoc M = Conexion.MovimientoDoc.FirstOrDefault(w => w.NoDocOrigen == NoDocumento  && w.TipoDocumentoOrigen == TipoDocumento && w.Esquema == "CXP")!;

           
                    datos = new();
                    datos.Nombre = "DOCUMENTO";
                    datos.d = M;
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
