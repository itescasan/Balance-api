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



                    json = Cls_Mensaje.Tojson(lstDatos, lstDatos.Count, string.Empty, string.Empty, 0);
                }



            }
            catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;
        }


        [Route("api/Contabilidad/Cheques/GetReembolsos")]
        [HttpGet]
        public string GetReembolsos()
        {
            return V_GetReembolsos();
        }

        private string V_GetReembolsos()
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new();

                    List<Reembolsos> qReembolso = Conexion.Reembolsos.FromSqlRaw<Reembolsos>($"select Distinct R.Fecha,C.Titulo, R.Numero from CONESCASAN..Reembolsos R inner join CONESCASAN..tbCostos C on C.Codigo = R.Ccosto where year(Fecha) = year(GETDATE()) and R.Contabilizado = 0").ToList();
                    
                    Cls_Datos datos = new();
                    datos.Nombre = "REEMBOLSOS";
                    datos.d = qReembolso;

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

        [Route("api/Contabilidad/Cheques/GetReembolsos")]
        [HttpPost]
        public IActionResult GetReembolso(int Numero)
        {
            if (ModelState.IsValid)
            {

                return Ok(V_GetReembolso(Numero));

            }
            else
            {
                return BadRequest();
            }

        }

        public string V_GetReembolso(int Numero)
        {



            string json = string.Empty;

            try
            {
                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new();

                    List<Reembolsos> qReembolso = Conexion.Reembolsos.FromSqlRaw<Reembolsos>($"select Distinct R.Fecha,C.Titulo, R.Numero from CONESCASAN..Reembolsos R inner join CONESCASAN..tbCostos C on C.Codigo = R.Ccosto where year(Fecha) = year(GETDATE()) and R.Contabilizado = 0").ToList();

                    Cls_Datos datos = new();
                    datos.Nombre = "REEMBOLSOS";
                    datos.d = qReembolso;

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
