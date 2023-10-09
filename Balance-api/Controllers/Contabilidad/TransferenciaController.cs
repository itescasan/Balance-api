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

    }
}
