using Balance_api.Class;
using Balance_api.Contexts;
using Balance_api.Controllers.Sistema;
using Balance_api.Models.Contabilidad;
using Balance_api.Models.Sistema;
using DevExpress.Data.ODataLinq.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Transactions;

namespace Balance_api.Controllers.Contabilidad
{
    public class CierreMensualController : Controller
    {
        private readonly BalanceEntities Conexion;

        public CierreMensualController(BalanceEntities db)
        {
            Conexion = db;
        }


        [Route("api/Contabilidad/CierreMensual")]
        [HttpPost]
        public IActionResult CierreMensual(string Codigo, DateTime Fecha)
        {
            if (ModelState.IsValid)
            {

                return Ok(V_CierreMensual(Codigo, Fecha));

            }
            else
            {
                return BadRequest();
            }

        }

        public string V_CierreMensual(string Codigo, DateTime Fecha)
        {



            string json = string.Empty;

            try
            {
                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                using (Conexion)
                {


                    string Sql1 = $"";
                    string Sql2 = $"";



                    switch (Codigo)
                    {
                        case "01"://Inventario
                            break;
                        case "02"://Facturacion
                            Sql1 = $" DECLARE @p_Retorno INT = 0,\r\n  \t@p_Mensaje NVARCHAR(500)= ''\r\n\r\nEXEC [CNT].[Sp_AsientoContableMasterFAC] {Fecha.Year}, {Fecha.Month}, 'FAC', @p_Retorno OUT, @p_Mensaje OUT\r\n\r\nSELECT @p_Retorno AS p_Retorno, @p_Mensaje AS p_Mensaje";
                            Sql2 = $" DECLARE @p_Retorno INT = 0,\r\n  \t@p_Mensaje NVARCHAR(500)= ''\r\n\r\nEXEC [CNT].[Sp_AsientoContableSlaveFAC] {Fecha.Year}, {Fecha.Month}, 'FAC', @p_Retorno OUT, @p_Mensaje OUT\r\n\r\nSELECT @p_Retorno AS p_Retorno, @p_Mensaje AS p_Mensaje";
                            break;
                        case "03"://Cuentas por Cobrar
                            Sql1 = $" DECLARE @p_Retorno INT = 0,\r\n  \t@p_Mensaje NVARCHAR(500)= ''\r\n\r\nEXEC [CNT].[Sp_AsientoContableMasterCXC] {Fecha.Year}, {Fecha.Month}, 'CXC', @p_Retorno OUT, @p_Mensaje OUT\r\n\r\nSELECT @p_Retorno AS p_Retorno, @p_Mensaje AS p_Mensaje";
                            Sql2 = $" DECLARE @p_Retorno INT = 0,\r\n  \t@p_Mensaje NVARCHAR(500)= ''\r\n\r\nEXEC [CNT].[Sp_AsientoContableSlaveCXC] {Fecha.Year}, {Fecha.Month}, 'CXC', @p_Retorno OUT, @p_Mensaje OUT\r\n\r\nSELECT @p_Retorno AS p_Retorno, @p_Mensaje AS p_Mensaje";
                            break;
                        case "04"://Cuentas por Pagar
                            break;
                        case "05"://Nomina
                            break;
                        case "06"://Caja
                            Sql1 = $" DECLARE @p_Retorno INT = 0,\r\n  \t@p_Mensaje NVARCHAR(500)= ''\r\n\r\nEXEC [CNT].[Sp_AsientoContableMasterCAJ] {Fecha.Year}, {Fecha.Month}, 'CAJ', @p_Retorno OUT, @p_Mensaje OUT\r\n\r\nSELECT @p_Retorno AS p_Retorno, @p_Mensaje AS p_Mensaje";
                            Sql2 = $" DECLARE @p_Retorno INT = 0,\r\n  \t@p_Mensaje NVARCHAR(500)= ''\r\n\r\nEXEC [CNT].[Sp_AsientoContableSlaveCAJ] {Fecha.Year}, {Fecha.Month}, 'CAJ', @p_Retorno OUT, @p_Mensaje OUT\r\n\r\nSELECT @p_Retorno AS p_Retorno, @p_Mensaje AS p_Mensaje";
                            break;
                        case "07"://Costos
                            break;
                    }


                    CierreMes Cierre = Conexion.CierreMes.FromSqlRaw(Sql1).ToList().First();

                    CierreMes Cierre2 = Conexion.CierreMes.FromSqlRaw(Sql2).ToList().First();





                    if (Cierre.p_Retorno == 0 && Cierre2.p_Retorno == 0)
                    {
                        Conexion.SaveChanges();
                        scope.Complete();
                        
                    }


                    Cls_Datos datos = new();
                    datos.Nombre = "CIERRE MES";
                    datos.d = string.Empty;
                    datos.d = Cierre.p_Mensaje == string.Empty ? Cierre2.p_Mensaje : Cierre.p_Mensaje;
                    if(Cierre.p_Retorno == 0 && Cierre2.p_Retorno == 0) datos.d = "Cierre Procesado";


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

    }
}
