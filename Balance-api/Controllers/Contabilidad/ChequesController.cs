using Balance_api.Class;
using Balance_api.Class.Contabilidad;
using Balance_api.Contexts;
using Balance_api.Controllers.Sistema;
using Balance_api.Models.Contabilidad;
using Balance_api.Models.Inventario;
using Balance_api.Models.Proveedor;
using Balance_api.Models.Sistema;
using DevExpress.Charts.Native;
using DevExpress.CodeParser;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Transactions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Balance_api.Controllers.Contabilidad
{
    public class ChequesController : Controller
    {
        private readonly BalanceEntities Conexion;

        public ChequesController(BalanceEntities db) 
        {
            Conexion = db;
        }


        [Route("api/Contabilidad/Cheques/Datos")]
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
                                           where _q.Activo && _q.Tipo == "C"
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
                                               _q.SerieDocumento.IdSerie,
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

                    var qCentroCosto = Conexion.CatalogoCentroCostos.ToList();


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


                    //string sQuery = $"select RTRIM(LTRIM(R.Numero)) + ' - ' + RTRIM(LTRIM(C.Titulo))Titulo from CONESCASAN..Reembolsos R inner join CONESCASAN..tbCostos C on C.Codigo = R.Ccosto where Contabilizado = 0 AND Aplicado = 0 GROUP BY C.Titulo,R.Numero,R.Ccosto order by RTRIM(LTRIM(R.Numero)) + ' - ' + RTRIM(LTRIM(C.Titulo)) desc\r\n";

                    //var qReembolso = Conexion.Reembolsos.FromSqlRaw(sQuery).ToList();


                    //var qReembolso = Conexion.Database.SqlQuery<Reembolsos>($"").ToList();

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
                                           DetalleCaja = _q.DetalleCaja.ToList()
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


        [Route("api/Contabilidad/Cheques/Guardar")]
        [HttpPost]
        public IActionResult Guardar([FromBody] Cls_Datos_Cheque d)
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

        [Route("api/Contabilidad/Cheques/GetDocumentos")]
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

                    List<ChequeDocumento> qDocumentos = (from _q in qDoc
                                                         where _q.Activo
                                                         group _q by new
                                                         {
                                                             NoDococumento = (_q.NoDocEnlace == null ? _q.NoDocOrigen : _q.NoDocEnlace),
                                                             Serie = (_q.NoDocEnlace == null ? _q.SerieOrigen : _q.SerieEnlace),
                                                             TipoDocumento = (_q.NoDocEnlace == null ? _q.TipoDocumentoOrigen : _q.TipoDocumentoEnlace),
                                                         } into grupo
                                                         select new ChequeDocumento
                                                         {
                                                             IdDetChequeDoc = new Guid(),
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



                    var Doc = qDocumentos.Select((file, index) => new
                    {
                        Index = index,
                        file.Documento,
                        file.Serie,
                        file.TipoDocumento,
                        file.Fecha,
                        file.IdMoneda,
                        file.TasaCambioDoc,
                        file.SaldoDolar,
                        file.SaldoCordoba
                    }).ToList();

                    Cls_Datos datos = new();
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

        private string V_Guardar(Cls_Datos_Cheque d)
        {

            string json = string.Empty;

            try
            {

                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                using (Conexion)
                {

                    bool esNuevo = false;
                    Cheques? _Chequ = Conexion.Cheque.Find(d.C.IdCheque);
                    string IdMonedaLocal = Conexion.Database.SqlQueryRaw<string>($"SELECT TOP 1 MonedaLocal FROM SIS.Parametros").ToList().First();
                    if (_Chequ == null)
                    {

                        Conexion.Database.ExecuteSqlRaw($"UPDATE CNT.SerieDocumentos SET Consecutivo += 1  WHERE  IdSerie = '{d.C.IdSerie}'");
                        Conexion.SaveChanges();

                        int ConsecutivoSerie = Conexion.Database.SqlQueryRaw<int>($"SELECT Consecutivo FROM CNT.SerieDocumentos WHERE IdSerie = '{d.C.IdSerie}'").ToList().First();

                        d.C.NoCheque = string.Concat(d.C.IdSerie, ConsecutivoSerie);


                        d.A.TipoDocOrigen = d.C.TipoCheque == "C" ? "CHEQUE A CUENTA" : "CHEQUE A DOCUMENTO";
                        d.A.NoDocOrigen = d.C.NoCheque;
                        d.A.NoAsiento = d.C.NoCheque;
                        d.A.IdSerie = d.C.IdSerie;
                        d.A.IdSerieDocOrigen = d.A.IdSerie;
                        d.A.Estado = "Autorizado";

                        _Chequ = new Cheques();
                        _Chequ.FechaReg = DateTime.Now;
                        _Chequ.UsuarioReg = d.C.UsuarioReg;
                        _Chequ.Anulado = false;
                        esNuevo = true;
                    }

                    _Chequ.IdCuentaBanco = d.C.IdCuentaBanco;
                    _Chequ.CuentaContable = d.C.CuentaContable;
                    _Chequ.IdMoneda = d.C.IdMoneda;
                    _Chequ.CentroCosto = d.C.CentroCosto;
                    _Chequ.CodBodega = d.C.CodBodega;
                    _Chequ.IdSerie = d.C.IdSerie;
                    _Chequ.NoCheque = d.C.NoCheque;
                    _Chequ.Fecha = d.C.Fecha;
                    _Chequ.Beneficiario = d.C.Beneficiario;
                    _Chequ.TasaCambio = d.C.TasaCambio;
                    _Chequ.Concepto = d.C.Concepto;
                    _Chequ.TipoCheque = d.C.TipoCheque;
                    _Chequ.CodBodega = d.C.CodBodega;
                    _Chequ.Total = d.C.Total;
                    _Chequ.TotalDolar = d.C.TotalDolar;
                    _Chequ.TotalCordoba = d.C.TotalCordoba;
                    _Chequ.CodProveedor = d.C.CodProveedor;
                    _Chequ.IdIngresoCaja = d.C.IdIngresoCaja;
                    _Chequ.CuentaIngCaja = d.C.CuentaIngCaja;

                    _Chequ.UsuarioUpdate = d.C.UsuarioReg;
                    _Chequ.FechaUpdate = DateTime.Now;
                    if (esNuevo) Conexion.Cheque.Add(_Chequ);

                    Conexion.SaveChanges();

                    if (_Chequ.IdIngresoCaja > 0)
                    {
                        IngresoCaja? det = Conexion.IngresoC.FirstOrDefault(f => f.IdIngresoCajaChica == _Chequ.IdIngresoCaja);

                        if (det != null)
                        {
                            //Conexion.DetIngCaja.Remove(det!);
                            det.Aplicado = true;
                            det.UsuarioModifica = _Chequ.UsuarioReg;
                            det.FechaModificacion = DateTime.Now;
                            Conexion.SaveChanges();
                            Conexion.Database.ExecuteSqlRaw($"UPDATE CNT.IngresosCajaChica SET Contabilizado = 1  WHERE  IdIngresoCajaChica = '{_Chequ.IdIngresoCaja}'");
                        }

                    }

                    if (d.C.ChequeDocumento != null)
                    {

                        //MovimientoDoc[] _mov = Conexion.MovimientoDoc.Where(w => w.NoDocOrigen == _Transf.NoTransferencia).ToArray();
                        //Conexion.MovimientoDoc.RemoveRange(_mov);
                        //Conexion.SaveChanges();


                        int i = 0;
                        foreach (ChequeDocumento doc in d.C.ChequeDocumento)
                        {
                            bool esNuevoDet = false;

                            ChequeDocumento? det = Conexion.ChequeDocumento.Find(doc.IdDetChequeDoc);

                            if (det == null)
                            {
                                esNuevoDet = true;
                                det = new ChequeDocumento();
                                det.IdDetChequeDoc = Guid.NewGuid();
                            }

                            det.IdCheque = _Chequ.IdCheque;
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


                            if (esNuevoDet) Conexion.ChequeDocumento.Add(det);


                            i++;


                            MovimientoDoc? mdoc = Conexion.MovimientoDoc.FirstOrDefault(ff => ff.NoDocOrigen == _Chequ.NoCheque && ff.SerieOrigen == _Chequ.IdSerie && ff.TipoDocumentoOrigen == "CHEQUE" && ff.NoDocEnlace == det.Documento && ff.TipoDocumentoEnlace == det.TipoDocumento && ff.Esquema == "CXP");
                            Bodegas? bo = Conexion.Bodegas.FirstOrDefault(ff => ff.Codigo == _Chequ.CodBodega);

                            if (mdoc != null)
                            {
                                Conexion.MovimientoDoc.Remove(mdoc);
                                Conexion.SaveChanges();
                            }

                            mdoc = new MovimientoDoc();
                            mdoc.NoMovimiento = string.Empty;
                            mdoc.IdBodega = (bo == null ? 0 : bo.IdBodega);
                            mdoc.CodigoBodega = (bo == null ? string.Empty : bo.Codigo);
                            mdoc.CodigoCliente = _Chequ.CodProveedor!;
                            mdoc.CodVendedor = string.Empty;
                            mdoc.NoDocOrigen = _Chequ.NoCheque;
                            mdoc.SerieOrigen = _Chequ.IdSerie;
                            mdoc.FechaDocumento = _Chequ.Fecha;
                            mdoc.Plazo = 30;
                            mdoc.DiaGracia = 0;
                            mdoc.TipoVenta = "Credito";
                            mdoc.TasaCambio = _Chequ.TasaCambio;
                            mdoc.TipoDocumentoOrigen = "CHEQUE";
                            mdoc.IdMoneda = _Chequ.IdMoneda;
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

                        foreach (ChequeRetencion doc in d.C.ChequeRetencion)
                        {
                            bool esNuevoDet = false;

                            ChequeRetencion? ret = Conexion.ChequeRetencion.Find(doc.IdDetRetencionCk);

                            if (ret == null)
                            {
                                esNuevoDet = true;
                                ret = new ChequeRetencion();
                                ret.IdDetRetencionCk = Guid.NewGuid();
                            }

                            ret.IdCheque = _Chequ.IdCheque;
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


                            if (esNuevoDet) Conexion.ChequeRetencion.Add(ret);


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
                        //d.C.TipoCheque == "C" ? "CHEQUE A CUENTA" : "CHEQUE A DOCUMENTO"
                        //_Asiento = Conexion.AsientosContables.FirstOrDefault(f => f.NoDocOrigen == _Chequ.NoCheque && f.IdSerieDocOrigen == d.C.IdSerie && f.TipoDocOrigen == (d.C.TipoCheque));
                        _Asiento = Conexion.AsientosContables.FirstOrDefault(f => f.NoDocOrigen == _Chequ.NoCheque && f.IdSerieDocOrigen == d.C.IdSerie && f.TipoDocOrigen == (d.C.TipoCheque == "C" ? "CHEQUE A CUENTA" : "CHEQUE A DOCUMENTO"));


                        _Asiento!.AsientosContablesDetalle = d.A.AsientosContablesDetalle;

                    }

                    AsientoController _Controller = new AsientoController(Conexion);
                    json = _Controller.V_Guardar(_Asiento!, Conexion, false);

                    Cls_JSON? reponse = JsonSerializer.Deserialize<Cls_JSON>(json);

                    if (reponse == null) return json;
                    if (reponse.esError == 1) return json;


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

        [Route("api/Contabilidad/Cheques/Get")]
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


                    var qCheques = (from _q in Conexion.Cheque
                                    where _q.CodBodega == (CodBodega == string.Empty ? _q.CodBodega : CodBodega) && _q.Fecha.Date >= Fecha1.Date && _q.Fecha <= Fecha2.Date
                                    select new
                                    {
                                        _q.IdCheque,
                                        _q.IdCuentaBanco,
                                        CuentaBancaria = string.Concat(_q.CuentaBanco.Bancos.Banco, " ", _q.CuentaBanco.NombreCuenta, " ", _q.CuentaBanco.Monedas.Simbolo, " ", _q.CuentaBanco.CuentaBancaria),
                                        _q.CuentaContable,
                                        _q.IdMoneda,
                                        _q.CodBodega,
                                        _q.IdSerie,
                                        _q.NoCheque,
                                        _q.Fecha,
                                        _q.Beneficiario,
                                        _q.CodProveedor,
                                        _q.TasaCambio,
                                        _q.Concepto,
                                        _q.TipoCheque,
                                        _q.Total,
                                        _q.TotalDolar,
                                        _q.TotalCordoba,
                                        _q.Anulado,
                                        _q.UsuarioReg,
                                        _q.FechaReg,
                                        _q.UsuarioUpdate,
                                        _q.UsuarioAnula,
                                        _q.FechaAnulacion,
                                        _q.IdIngresoCaja,
                                        _q.CuentaIngCaja
                                    }).ToList();




                    Cls_Datos datos = new();
                    datos.Nombre = "CHEQUE";
                    datos.d = qCheques;

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



        [Route("api/Contabilidad/Cheques/GetDetalleCuenta")]
        [HttpGet]
        public string GetDetalleCuenta(Guid Idcheque)
        {
            return V_GetDetalleCuenta(Idcheque);
        }

        private string V_GetDetalleCuenta(Guid Idcheque)
        {

            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new();

                    Cheques T = Conexion.Cheque.Find(Idcheque)!;

                    var A = (from _q in Conexion.AsientosContables
                             where _q.NoDocOrigen == T.NoCheque && _q.IdSerieDocOrigen == T.IdSerie && _q.TipoDocOrigen == "CHEQUE A CUENTA"
                             select _q.AsientosContablesDetalle).ToList();




                    Cls_Datos datos = new();
                    datos.Nombre = "DETALLE CHEQUE";
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

        [Route("api/Contabilidad/Cheques/GetDetalleDocumentos")]
        [HttpGet]
        public string GetDetalleDocumentos(Guid Idcheque)
        {
            return V_GetDetalleDocumentos(Idcheque);
        }

        private string V_GetDetalleDocumentos(Guid Idcheque)
        {

            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new();

                    Cheques C = Conexion.Cheque.Find(Idcheque)!;



                    var qDocumentos = (from _q in Conexion.Cheque
                                       where _q.IdCheque == Idcheque
                                       select _q.ChequeDocumento).First();





                    Cls_Datos datos = new();
                    datos.Nombre = "DETALLE DOCUMENTOS";
                    datos.d = qDocumentos;
                    lstDatos.Add(datos);


                    var qRetenciones = (from _q in Conexion.Cheque
                                        where _q.IdCheque == Idcheque
                                        select _q.ChequeRetencion).First();



                    datos = new();
                    datos.Nombre = "RETENCIONES";
                    datos.d = qRetenciones;

                    lstDatos.Add(datos);


                    var A = (from _q in Conexion.AsientosContables
                             where _q.NoDocOrigen == C.NoCheque && _q.IdSerieDocOrigen == C.IdSerie && _q.TipoDocOrigen == "CHEQUE A DOCUMENTO"
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
                                 _q.UsuarioReg

                             }).ToList();

                    datos = new();
                    datos.Nombre = "ASIENTO";
                    datos.d = A.First();

                    lstDatos.Add(datos);




                    var D = (from _q in Conexion.AsientosContables
                             where _q.NoDocOrigen == C.NoCheque && _q.IdSerieDocOrigen == C.IdSerie && _q.TipoDocOrigen == "CHEQUE A DOCUMENTO"
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

        [Route("api/Contabilidad/Cheques/GetDetalleReembolso")]
        [HttpGet]
        public string GetDetalleReembolso(int id)
        {
            return V_GetDetalleReembolso(id);
        }

        private string V_GetDetalleReembolso(int id)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new();

                    var qReembolsoD = (from _q in Conexion.DetIngCaja
                                       where _q.IdIngresoCajaC == id                                       
                                       select new
                                       {
                                        _q.IdDetalleIngresoCajaChica,
                                        _q.Cuenta,
                                        _q.Referencia,
                                        _q.CentroCosto,
                                        _q.SubTotal,
                                        _q.Iva,
                                        _q.Total
                                       }).ToList();

                    Cls_Datos datos = new();
                    datos = new();
                    datos.Nombre = "ReembolsoD";
                    datos.d = qReembolsoD;
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

