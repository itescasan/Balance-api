using Balance_api.Class;
using Balance_api.Class.Contabilidad;
using Balance_api.Contexts;
using Balance_api.Controllers.Sistema;
using Balance_api.Models.Contabilidad;
using Balance_api.Models.Sistema;
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
                                  select new
                                  {
                                      _q.IdCuentaBanco,
                                      _q.Bancos.Banco,
                                      _q.CuentaBancaria,
                                      _q.NombreCuenta,
                                      _q.IdMoneda,
                                      _q.Monedas.Moneda,
                                      _q.Bancos.CuentaC,
                                      _q.Bancos.CuentaD,
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
                   
                    if (_Transf == null)
                    {

                        Conexion.Database.ExecuteSqlRaw($"UPDATE CNT.SerieDocumentos SET Consecutivo += 1  WHERE  IdSerie = '{d.T.IdSerie}'");
                        Conexion.SaveChanges();

                        int ConsecutivoSerie = Conexion.Database.SqlQueryRaw<int>($"SELECT Consecutivo FROM CNT.SerieDocumentos WHERE IdSerie = '{d.T.IdSerie}'").ToList().First();

                        d.T.NoTransferencia = string.Concat(d.T.IdSerie, ConsecutivoSerie);
        

                        d.A.TipoDocOrigen = "TRANSFERENCIA A CUENTA";
                        d.A.NoDocOrigen = d.T.NoTransferencia;
                        d.A.NoAsiento = d.T.NoTransferencia;
                        d.A.IdSerie = d.T.IdSerie;
                        d.A.IdSerieDocOrigen = d.A.IdSerie;
                        d.A.Estado = "Autorizado";

                        _Transf = new Transferencia();
                        _Transf.FechaReg = DateTime.Now;
                        _Transf.UsuarioReg = d.T.UsuarioReg;
                        _Transf.Anulado = false;
                        esNuevo = true;
                    }

                    _Transf.IdCuentaBanco = d.T.IdCuentaBanco;
                    _Transf.CodBodega = d.T.CodBodega;
                    _Transf.IdSerie = d.T.IdSerie;
                    _Transf.NoTransferencia = d.T.NoTransferencia;
                    _Transf.Fecha = d.T.Fecha;
                    _Transf.Beneficiario = d.T.Beneficiario;
                    _Transf.TasaCambio = d.T.TasaCambio;
                    _Transf.Concepto = d.T.Concepto;
                    _Transf.TipoTransferencia = d.T.TipoTransferencia;
                    _Transf.CodBodega = d.T.CodBodega;
                    _Transf.Total = d.T.Total;
                    _Transf.TotalDolar = d.T.TotalDolar;
                    _Transf.TotalCordoba = d.T.TotalCordoba;


                    _Transf.UsuarioUpdate = d.T.UsuarioReg;
                    _Transf.FechaUpdate = DateTime.Now;
                    if (esNuevo) Conexion.Transferencia.Add(_Transf);

                    Conexion.SaveChanges();

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




        [Route("api/Contabilidad/Transferencia/Get")]
        [HttpGet]
        public string Get(DateTime Fecha1, DateTime Fecha2, string CodBodega)
        {
            return V_Get(Fecha1, Fecha2, CodBodega);
        }

        private string V_Get(DateTime Fecha1, DateTime Fecha2, string CodBodega)
        {
            if (CodBodega == null) CodBodega = string.Empty;

            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new();


                    var qTransferencia = (from _q in Conexion.Transferencia
                                          where _q.CodBodega == (CodBodega == string.Empty ? _q.CodBodega : CodBodega)
                                          select new
                                          {
                                              _q.IdTransferencia,
                                              _q.IdCuentaBanco,
                                              CuentaBancaria = string.Concat(_q.CuentaBanco.Bancos.Banco, " ", _q.CuentaBanco.NombreCuenta, " ", _q.CuentaBanco.Monedas.Simbolo, " ", _q.CuentaBanco.CuentaBancaria),
                                              _q.CodBodega,
                                              _q.IdSerie,
                                              _q.NoTransferencia,
                                              _q.Fecha,
                                              _q.Beneficiario,
                                              _q.TasaCambio,
                                              _q.Concepto,
                                              _q.TipoTransferencia,
                                              _q.Total,
                                              _q.TotalDolar,
                                              _q.TotalCordoba,
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


    }
}
