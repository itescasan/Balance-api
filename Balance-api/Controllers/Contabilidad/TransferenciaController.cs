using Balance_api.Class;
using Balance_api.Class.Contabilidad;
using Balance_api.Contexts;
using Balance_api.Controllers.Sistema;
using Balance_api.Models.Contabilidad;
using Balance_api.Models.Inventario;
using Balance_api.Models.Proveedor;
using Balance_api.Models.Sistema;
using Balance_api.Reporte.Contabilidad;
using DevExpress.Charts.Native;
using DevExpress.DataAccess.Sql;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
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

                    List<Cls_Datos> lstDatos = new List<Cls_Datos>();

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
                                           SaldoDolar = grupo.Sum(s => s.TotalDolar),
                                           Seleccionar = false,
                                          
                                       }).ToList();


                    qDocumentos = qDocumentos.Where(w => w.SaldoCordoba > 0).ToList();

                 

                    var Doc = qDocumentos.Select((file, index) => new { Index = index,  file.Documento,  file.Serie,file.TipoDocumento,
                         file.Fecha,  file.IdMoneda, file.TasaCambioDoc,  file.SaldoDolar,  file.SaldoCordoba,
                        file.Seleccionar
                    }).ToList();

                    Cls_Datos  datos = new();
                    datos.Nombre = "DOC PROVEEDOR";
                    datos.d = Doc;
                    lstDatos.Add(datos);

               


                    //var qOrdenComp = (from _q in  Conexion.OrdenCompra.ToList() 
                    //                  join _i in Conexion.CuentaXPagar on _q.IdOrdenCompra equals _i.IdOrdenCompra
                    //                  join _d in qDocumentos on new { DOC = _i.NoSolicitud, TIPO = _q.TipoDocOrigen } equals new { DOC = _d.Documento, TIPO = _d.TipoDocumento }
                    //                  where _q.CodigoProveedor == CodProveedor && _q.Estado == "APROBADO"
                    //                  select new
                    //                  {
                    //                      NoDocOrigen = _i.NoSolicitud,
                    //                      TipoDocOrigen = _q.TipoDocOrigen,
                    //                      Participacion1 = 100,
                    //                      Participacion2 = 100,
                    //                      CuentaContable = _q.CuentaContableSolicitante,
                    //                      Bodega = _q.CodigoBodega,
                    //                      CentroCosto = string.Empty
                    //                  }).ToList();




                    var qOrdenComp = (from _q in Conexion.OrdenCompraCentrogasto.ToList()
                                      join _i in Conexion.OrdenCompra.ToList() on _q.IdOrdenCompra equals _i.IdOrdenCompra
                                      join _d in qDocumentos on new { DOC = _i.NoOrdenCompra, TIPO = _q.TipoDocOrigen } equals new { DOC = _d.Documento, TIPO = _d.TipoDocumento }
                                      where _i.CodigoProveedor == CodProveedor && _i.Estado == "APROBADO" && _q.TipoDocOrigen == "GASTO_ANT"
                                      select new
                                      {
                                          NoDocOrigen = _i.NoOrdenCompra,
                                          _q.TipoDocOrigen,
                                          _q.Participacion1,
                                          _q.Participacion2,
                                          _i.CuentaContableSolicitante,
                                          _q.CuentaContable,
                                          _q.Bodega,
                                          _q.CentroCosto,
                                          _i.SubTotal,
                                          _i.SubTotalDolar,
                                          _i.SubTotalCordoba
                                      }).Union(

                        from _q in Conexion.OrdenCompraCentrogasto.ToList()
                        join _i in Conexion.OrdenCompra.ToList() on _q.IdOrdenCompra equals _i.IdOrdenCompra
                        join _d in qDocumentos on new { DOC = _q.NoDocOrigen, TIPO = _q.TipoDocOrigen } equals new { DOC = _d.Documento, TIPO = _d.TipoDocumento }
                        where _i.CodigoProveedor == CodProveedor && _i.Estado == "APROBADO" && _q.TipoDocOrigen == ""
                        select new
                        {
                            _q.NoDocOrigen,
                            _q.TipoDocOrigen,
                            _q.Participacion1,
                            _q.Participacion2,
                            i.CuentaContableSolicitante,
                            CuentaContable = string.Empty,
                            _q.Bodega,
                            _q.CentroCosto,
                            _i.SubTotal,
                            _i.SubTotalDolar,
                            _i.SubTotalCordoba
                        }


                        ).ToList();




                    datos = new();
                    datos.Nombre = "DOC ORDEN COMPRA";
                    datos.d = qOrdenComp;
                    lstDatos.Add(datos);





                    var qAnticipos = (from _q in qDoc
                                      where _q.Activo && _q.TipoDocumentoEnlace == "GASTO_ANT"
                                      group _q by new
                                      {
                                          NoDococumento = _q.NoDocEnlace,
                                          Serie = _q.SerieEnlace,
                                          TipoDocumento = _q.TipoDocumentoEnlace,
                                      } into grupo
                                      select new
                                      {
                                          Documento = grupo.Key.NoDococumento,
                                          grupo.Key.Serie,
                                          grupo.Key.TipoDocumento,
                                          AnticipoCordoba = Math.Abs(grupo.Sum(s => s.TotalCordoba)),
                                          AnticipoDolar = Math.Abs(grupo.Sum(s => s.TotalDolar))

                                      }).ToList();


                    datos = new();
                    datos.Nombre = "ANTICIPO";
                    datos.d = qAnticipos;
                    lstDatos.Add(datos);


                    List<Retenciones> R  = Conexion.Retenciones.Where(w => w.AplicaEnCXC == false && w.AplicarAutomatico == true).ToList();
                    List<TranferenciaRetencion> lstRetenciones = new();


                    int index = 0;
                    qDocumentos.ForEach(doc => {


                        R.ForEach(w => {

                            var _doc = qDoc.Find(f => f.NoDocOrigen == doc.Documento && f.SerieOrigen == doc.Serie && f.TipoDocumentoOrigen == doc.TipoDocumento);



                            TranferenciaRetencion _r = new();
                            _r.IdDetRetencion = new Guid();
                            _r.IdTransferencia = new Guid();
                            _r.Index = index;
                            _r.Retencion = w.Retencion;
                            _r.Porcentaje =  Convert.ToDecimal(w.Porcentaje);
                            _r.Documento = doc.Documento;
                            _r.Serie = doc.Serie;
                            _r.TipoDocumento = doc.TipoDocumento;
                            _r.IdMoneda = string.Empty;
                            _r.TasaCambio = 0;
                            _r.SubTotal = _doc!.SubTotal;
                            _r.SubTotalML = _doc!.SubTotalCordoba;
                            _r.SubTotalMS = _doc!.SubTotalDolar;
                            _r.Monto = _r.SubTotal * (  _r.Porcentaje / 100);
                            _r.TieneImpuesto = _doc.Impuesto == 0 ? false : true;
                            _r.PorcImpuesto = 0;
                            if (_r.TieneImpuesto) _r.PorcImpuesto =  Math.Round( _doc.Impuesto / _doc.SubTotal, 2, MidpointRounding.ToEven);
                            _r.CuentaContable = w.CuentaContable!;
                            _r.RetManual = false;
                            _r.Naturaleza = w.Naturaleza!;

                            lstRetenciones.Add(_r );

                            index++;
                        });




                    });

                   





                    datos = new();
                    datos = new();
                    datos.Nombre = "RETENCIONES";
                    datos.d = lstRetenciones;
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
                    _Transf.CentroCosto = d.T.CentroCosto;
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
                            det.Seleccionar = false;


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
                            ret.SubTotal = doc.SubTotal;
                            ret.SubTotalMS = doc.SubTotalMS;
                            ret.SubTotalML = doc.SubTotalML;
                            ret.Monto = doc.Monto;
                            ret.MontoMS = doc.MontoMS;
                            ret.MontoML = doc.MontoML;
                            ret.PorcImpuesto = doc.PorcImpuesto;
                            ret.TieneImpuesto = doc.TieneImpuesto;
                            ret.CuentaContable = doc.CuentaContable;
                            ret.RetManual = doc.RetManual;
                            ret.Naturaleza = doc.Naturaleza;


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


                    _Asiento = Conexion.AsientosContables.FirstOrDefault(f => f.NoDocOrigen == _Transf.NoTransferencia && f.IdSerieDocOrigen == d.T.IdSerie && f.TipoDocOrigen == (d.T.TipoTransferencia == "C" ? "TRANSFERENCIA A CUENTA" : "TRANSFERENCIA A DOCUMENTO"));


                    List<Cls_Datos> lstDatos = new();




                    xrpAsientoContable rpt = new xrpAsientoContable();

                    SqlDataSource sqlDataSource = (SqlDataSource)rpt.DataSource;
                    sqlDataSource.Connection.ConnectionString = Conexion.Database.GetConnectionString();


                    sqlDataSource.Queries["CNT_RPT_AsientoContable"].Parameters["@P_IdAsiento"].Value = _Asiento.IdAsiento;
                    sqlDataSource.Queries["CNT_RPT_AsientoContable"].Parameters["@P_IdMoneda"].Value = _Asiento.IdMoneda;
                    sqlDataSource.Queries["CNT_RPT_AsientoContable"].Parameters["@P_Consolidado"].Value = false;

                    MemoryStream stream = new MemoryStream();

                    rpt.ExportToPdf(stream, null);
                    stream.Seek(0, SeekOrigin.Begin);

                    Cls_Datos datos = new();
                    datos.d = stream.ToArray();
                    datos.Nombre = "REPORTE ASIENTO";
                    lstDatos.Add(datos);



                     datos = new();
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
                                          orderby _q.FechaReg descending
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



                    var qDocumentos = (from _q in Conexion.TransferenciaDocumento
                                       where _q.IdTransferencia == IdTransferencia
                                       select new {
                                           Seleccionar = false,
                                           _q.IdDetTrasnfDoc,
                                           _q.IdTransferencia,
                                           _q.Index,
                                           _q.Operacion,
                                           _q.Documento,
                                           _q.Serie,
                                           _q.TipoDocumento,
                                           _q.Fecha,
                                           _q.IdMoneda,
                                           _q.TasaCambioDoc,
                                           _q.SaldoAnt,
                                           _q.SaldoAntML,
                                           _q.SaldoAntMS,
                                           _q.Saldo,
                                           _q.SaldoDolar,
                                           _q.SaldoCordoba,
                                           _q.Importe,
                                           _q.ImporteML,
                                           _q.ImporteMS,
                                           _q.NuevoSaldo,
                                           _q.NuevoSaldoML,
                                           _q.NuevoSaldoMS,
                                           _q.DiferencialML,
                                           _q.DiferencialMS,
                                           _q.Retenido
                                       }).ToList();

                     



                    Cls_Datos datos = new();
                    datos.Nombre = "DETALLE DOCUMENTOS";
                    datos.d = qDocumentos;
                    lstDatos.Add(datos);



                    var qRetenciones = (from _q in Conexion.TranferenciaRetencion
                                       where _q.IdTransferencia == IdTransferencia
                                       select new {
                                           _q.IdDetRetencion,
                                           _q.IdTransferencia,
                                           _q.Index,
                                           _q.Retencion,
                                           _q.Porcentaje,
                                           _q.Documento,
                                           _q.Serie,
                                           _q.TipoDocumento,
                                           _q.IdMoneda,
                                           _q.TasaCambio,
                                           _q.SubTotal,
                                           _q.SubTotalMS,
                                           _q.SubTotalML,
                                           _q.Monto,
                                           _q.MontoMS,
                                           _q.MontoML,
                                           _q.PorcImpuesto,
                                           _q.TieneImpuesto,
                                           _q.CuentaContable,
                                           _q.RetManual,
                                           _q.Naturaleza

                                       }).ToList();



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


                    var D = (from _q in Conexion.AsientosContablesDetalle
                             where _q.Asiento!.NoDocOrigen == T.NoTransferencia && _q.Asiento.IdSerieDocOrigen == T.IdSerie && _q.Asiento.TipoDocOrigen == "TRANSFERENCIA A DOCUMENTO"
                             select new {

                                 _q.IdDetalleAsiento,
                                 _q.IdAsiento,
                                 _q.NoLinea,
                                 _q.CuentaContable,
                                 _q.Debito,
                                 _q.DebitoML,
                                 _q.DebitoMS,
                                 _q.Credito,
                                 _q.CreditoML,
                                 _q.CreditoMS,
                                 _q.Modulo,
                                 _q.Descripcion,
                                 _q.Referencia,
                                 _q.Naturaleza,
                                 _q.CentroCosto,
                                 _q.NoDocumento,
                                 _q.TipoDocumento,

                             }).ToList();




                    datos = new();
                    datos.Nombre = "DETALLE ASIENTO";
                    datos.d = D;

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
        public string BuscarTiposRetenciones(string NoDocumento)
        {
            return V_BuscarTiposRetenciones(NoDocumento);
        }

        private string V_BuscarTiposRetenciones(string NoDocumento)
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

                    string[] Docs = NoDocumento.Split(';');
                    List<MovimientoDoc> M = Conexion.MovimientoDoc.Where(w => Docs.Contains(string.Concat(w.NoDocOrigen, w.TipoDocumentoOrigen)) &&   w.Esquema == "CXP").ToList();

           
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




        [Route("api/Contabilidad/Transferencia/GetReporteAsiento")]
        [HttpGet]
        public string GetReporteAsiento(Guid IdTransferencia)
        {
            return V_GetReporteAsiento(IdTransferencia);
        }

        private string V_GetReporteAsiento(Guid IdTransferencia)
        {

            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                  
                    Transferencia T = Conexion.Transferencia.Find(IdTransferencia)!;

                    Asiento _Asiento = Conexion.AsientosContables.FirstOrDefault(f => f.NoDocOrigen == T.NoTransferencia && f.IdSerieDocOrigen == T.IdSerie && f.TipoDocOrigen == (T.TipoTransferencia == "C" ? "TRANSFERENCIA A CUENTA" : "TRANSFERENCIA A DOCUMENTO"))!;



                    xrpAsientoContable rpt = new xrpAsientoContable();

                    SqlDataSource sqlDataSource = (SqlDataSource)rpt.DataSource;
                    sqlDataSource.Connection.ConnectionString = Conexion.Database.GetConnectionString();


                    sqlDataSource.Queries["CNT_RPT_AsientoContable"].Parameters["@P_IdAsiento"].Value = _Asiento.IdAsiento;
                    sqlDataSource.Queries["CNT_RPT_AsientoContable"].Parameters["@P_IdMoneda"].Value = _Asiento.IdMoneda;

                    MemoryStream stream = new MemoryStream();

                    rpt.ExportToPdf(stream, null);
                    stream.Seek(0, SeekOrigin.Begin);

                    Cls_Datos datos = new();
                    datos.d = stream.ToArray();
                    datos.Nombre = "REPORTE ASIENTO";
     




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
