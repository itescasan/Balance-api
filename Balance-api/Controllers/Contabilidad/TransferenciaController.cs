using Balance_api.Class;
using Balance_api.Contexts;
using Balance_api.Controllers.Sistema;
using Balance_api.Models.Contabilidad;
using Balance_api.Models.Sistema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Transactions;

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
                                           where _q.Activo
                                  select new
                                  {
                                      _q.IdCuentaBanco,
                                      _q.Bancos.Banco,
                                      _q.CuentaBancaria,
                                      _q.NombreCuenta,
                                      _q.IdMoneda,
                                      _q.Monedas.Moneda,
                                      ConsecutivoCheque = _q.ConsecutivoCheque + 1,
                                      ConsecutivoTransferencia = _q.ConsecutivoTransferencia + 1,
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
        public IActionResult Guardar([FromBody] object[] d)
        {
            if (ModelState.IsValid)
            {
                Transferencia T = (Transferencia)d[0];
                Asiento A = (Asiento)d[1];

                return Ok(V_Guardar(T, A));

            }
            else
            {
                return BadRequest();
            }

        }

        private string V_Guardar(Transferencia d, Asiento a)
        {

            string json = string.Empty;

            try
            {

                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                using (Conexion)
                {

                    bool esNuevo = false;
                    Transferencia? _Transf = Conexion.Transferencia.Find(d.IdTransferencia);
                   
                    if (_Transf == null)
                    {

                        Conexion.Database.ExecuteSqlRaw($"UPDATE CNT.CuentaBanco SET ConsecutivoTransferencia += 1  WHERE  IdCuentaBanco = {d.IdCuentaBanco}");
                        Conexion.SaveChanges();

                        int Consecutivo = Conexion.Database.SqlQueryRaw<int>($"SELECT ConsecutivoTransferencia FROM CNT.CuentaBanco WHERE IdCuentaBanco = {d.IdCuentaBanco}").ToList().First();

                        _Transf = new Transferencia();
                        _Transf.FechaReg = DateTime.Now;
                        _Transf.UsuarioReg = d.UsuarioReg;
                        _Transf.Anulado = false;
                        esNuevo = true;
                    }

                    _Transf.CodBodega = d.CodBodega;
                    _Transf.NoTransferencia = d.NoTransferencia;
                    _Transf.Fecha = d.Fecha;
                    _Transf.Beneficiario = d.Beneficiario;
                    _Transf.TasaCambio = d.TasaCambio;
                    _Transf.Concepto = d.Concepto;
                    _Transf.TipoTransferencia = d.TipoTransferencia;
                    _Transf.CodBodega = d.CodBodega;
                    _Transf.CodBodega = d.CodBodega;

                    _Transf.UsuarioUpdate = d.UsuarioReg;
                    _Transf.FechaUpdate = DateTime.Now;
                    if (esNuevo) Conexion.Transferencia.Add(_Transf);

                    Conexion.SaveChanges();

                    Asiento? _Asiento = null;

                    if (esNuevo)
                    {
                        _Asiento = a;
                    }
                    else
                    {
                        _Asiento = Conexion.AsientosContables.FirstOrDefault(f => f.NoDocOrigen == _Transf.NoTransferencia && f.IdSerieDocOrigen == "Transfer" && f.TipoDocOrigen == (d.TipoTransferencia == "C" ? "TRANSFERENCIA A CUENTA" : "TRANSFERENCIA A DOCUMENTO"));

                    }

                    AsientoController _Controller = new AsientoController(Conexion);
                    json = _Controller.V_Guardar(_Asiento!, Conexion);

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
