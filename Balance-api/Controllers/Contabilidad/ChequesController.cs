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
                                               Consecutivo = string.Concat(_q.IdSerie, _q.SerieDocumento.Consecutivo + 1),
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

                    if (_Chequ == null)
                    {

                        Conexion.Database.ExecuteSqlRaw($"UPDATE CNT.SerieDocumentos SET Consecutivo += 1  WHERE  IdSerie = '{d.C.IdSerie}'");
                        Conexion.SaveChanges();

                        int ConsecutivoSerie = Conexion.Database.SqlQueryRaw<int>($"SELECT Consecutivo FROM CNT.SerieDocumentos WHERE IdSerie = '{d.C.IdSerie}'").ToList().First();

                        d.C.NoCheque = string.Concat(d.C.IdSerie, ConsecutivoSerie);


                        d.A.TipoDocOrigen = "Cheque";
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
                    //_Chequ.CentroCosto = d.C.CentroCosto;
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


                    _Chequ.UsuarioUpdate = d.C.UsuarioReg;
                    _Chequ.FechaUpdate = DateTime.Now;
                    if (esNuevo) Conexion.Cheque.Add(_Chequ);

                    Conexion.SaveChanges();

                    Asiento? _Asiento = null;

                    if (esNuevo)
                    {
                        _Asiento = d.A;
                    }
                    else
                    {
                        _Asiento = Conexion.AsientosContables.FirstOrDefault(f => f.NoDocOrigen == _Chequ.NoCheque && f.IdSerieDocOrigen == d.C.IdSerie && f.TipoDocOrigen == (d.C.TipoCheque));

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
            if (CodBodega == null) CodBodega = string.Empty;

            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new();


                    var qCheques = (from _q in Conexion.Cheque
                                          where _q.CodBodega == (CodBodega == string.Empty ? _q.CodBodega : CodBodega)
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
                                              _q.TasaCambio,
                                              _q.Concepto,
                                              _q.TipoCheque,
                                              _q.Total,
                                              _q.TotalDolar,
                                              _q.TotalCordoba,
                                              _q.Anulado,
                                              _q.UsuarioReg,
                                              _q.FechaReg
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
                                                                    SaldoCordoba = grupo.Sum(s => s.TotalCordoba),
                                                                    SaldoDolar = grupo.Sum(s => s.TotalDolar)
                                                                }).ToList();


                    qDocumentos = qDocumentos.Where(w => w.SaldoCordoba > 0).ToList();



                    var Doc = qDocumentos.Select((file, index) => new {
                        Index = index,
                        Documento = file.Documento,
                        Serie = file.Serie,
                        TipoDocumento = file.TipoDocumento,
                        Fecha = file.Fecha,
                        IdMoneda = file.IdMoneda,
                        SaldoDolar = file.SaldoDolar,
                        SaldoCordoba = file.SaldoCordoba
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
                             where _q.NoDocOrigen == T.NoCheque && _q.IdSerieDocOrigen == T.IdSerie && _q.TipoDocOrigen == "Cheque"
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


        //[Route("api/Contabilidad/Cheques/GetReembolsos")]
        //[HttpGet]
        //public string GetReembolsos()
        //{
        //    return V_GetReembolsos();
        //}

        //private string V_GetReembolsos()
        //{
        //    string json = string.Empty;
        //    try
        //    {
        //        using (Conexion)
        //        {
        //            List<Cls_Datos> lstDatos = new();

        //            List<Reembolsos> qReembolso = Conexion.Reembolsos.FromSqlRaw<Reembolsos>($"select Distinct R.Fecha,C.Titulo, R.Numero from CONESCASAN..Reembolsos R inner join CONESCASAN..tbCostos C on C.Codigo = R.Ccosto where year(Fecha) = year(GETDATE()) and R.Contabilizado = 0").ToList();

        //            Cls_Datos datos = new();
        //            datos.Nombre = "REEMBOLSOS";
        //            datos.d = qReembolso;

        //            lstDatos.Add(datos);


        //            json = Cls_Mensaje.Tojson(lstDatos, lstDatos.Count, string.Empty, string.Empty, 0);
        //        }



        //    }
        //    catch (Exception ex)
        //    {
        //        json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
        //    }

        //    return json;
        //}

        //[Route("api/Contabilidad/Cheques/GetReembolsos")]
        //[HttpPost]
        //public IActionResult GetReembolso(int Numero)
        //{
        //    if (ModelState.IsValid)
        //    {

        //        return Ok(V_GetReembolso(Numero));

        //    }
        //    else
        //    {
        //        return BadRequest();
        //    }

        //}

        //public string V_GetReembolso(int Numero)
        //{



        //    string json = string.Empty;

        //    try
        //    {
        //        using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
        //        using (Conexion)
        //        {
        //            List<Cls_Datos> lstDatos = new();

        //            List<Reembolsos> qReembolso = Conexion.Reembolsos.FromSqlRaw<Reembolsos>($"select Distinct R.Fecha,C.Titulo, R.Numero from CONESCASAN..Reembolsos R inner join CONESCASAN..tbCostos C on C.Codigo = R.Ccosto where year(Fecha) = year(GETDATE()) and R.Contabilizado = 0").ToList();

        //            Cls_Datos datos = new();
        //            datos.Nombre = "REEMBOLSOS";
        //            datos.d = qReembolso;

        //            lstDatos.Add(datos);


        //            json = Cls_Mensaje.Tojson(lstDatos, lstDatos.Count, string.Empty, string.Empty, 0);
        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
        //    }

        //    return json;

        //}




    }
}
