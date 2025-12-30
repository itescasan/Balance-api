using Balance_api.Class;
using Balance_api.Contexts;
using Balance_api.Controllers.Sistema;
using Balance_api.Models.Contabilidad;
using Balance_api.Models.Sistema;
using DevExpress.Data.ODataLinq.Helpers;
using DevExpress.Utils.Filtering.Internal;
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
        public IActionResult CierreMensual(string Codigo, DateTime Fecha,string Usuario)
        {
            if (ModelState.IsValid)
            {

                return Ok(V_CierreMensual(Codigo, Fecha, Usuario));

            }
            else
            {
                return BadRequest();
            }

        }

        public string V_CierreMensual(string Codigo, DateTime Fecha, string Usuario)
        {



            string json = string.Empty;

            try
            {
                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                using (Conexion)
                {


                    string Sql1 = $"";
                    string Sql2 = $"";
                    string Sql3 = $"";
                    int? cont = Conexion.Database.SqlQueryRaw<int?>($"SELECT Consecutivo FROM CNT.CierreProcesado WHERE Mes = {Fecha.Month} AND Anio = {Fecha.Year}").FirstOrDefault();
                    string col = string.Empty;
                    bool Completado = false;
                    DateTime f1 = new DateTime(Fecha.Year, Fecha.Month, 1);
                    DateTime f2 = f1.AddMonths(1).AddDays(-1);



                    //Conexion.Database.ExecuteSqlRaw("DISABLE TRIGGER TR_AUDITORIA_CNT_AsientosContablesDetalle ON  CNT.AsientosContablesDetalle;");
                    //Conexion.Database.ExecuteSqlRaw("DISABLE TRIGGER TR_AUDITORIA_CNT_AsientosContables ON  CNT.AsientosContables;");
                    //Conexion.SaveChanges();






                    switch (Codigo)
                    {
                        case "01"://Inventario
                            break;
                        case "02"://Facturacion
                            Sql1 = $" DECLARE @p_Retorno INT = 0,\r\n  \t@p_Mensaje NVARCHAR(500)= ''\r\n\r\nEXEC [CNT].[Sp_AsientoContableMasterFAC] {Fecha.Year}, {Fecha.Month}, 'FAC', @p_Retorno OUT, @p_Mensaje OUT\r\n\r\nSELECT @p_Retorno AS p_Retorno, @p_Mensaje AS p_Mensaje";
                            Sql2 = $" DECLARE @p_Retorno INT = 0,\r\n  \t@p_Mensaje NVARCHAR(500)= ''\r\n\r\nEXEC [CNT].[Sp_AsientoContableSlaveFAC] {Fecha.Year}, {Fecha.Month}, 'FAC', @p_Retorno OUT, @p_Mensaje OUT\r\n\r\nSELECT @p_Retorno AS p_Retorno, @p_Mensaje AS p_Mensaje";
                            Sql3 = $"EXEC CNT.CierreMes_Merge 'FAC', {Fecha.Year}, {Fecha.Month}";
                            col = "Facturacion";
                            break;
                        case "03"://Cuentas por Cobrar
                            Sql1 = $" DECLARE @p_Retorno INT = 0,\r\n  \t@p_Mensaje NVARCHAR(500)= ''\r\n\r\nEXEC [CNT].[Sp_AsientoContableMasterCXC] {Fecha.Year}, {Fecha.Month}, 'CXC', @p_Retorno OUT, @p_Mensaje OUT\r\n\r\nSELECT @p_Retorno AS p_Retorno, @p_Mensaje AS p_Mensaje";
                            Sql2 = $" DECLARE @p_Retorno INT = 0,\r\n  \t@p_Mensaje NVARCHAR(500)= ''\r\n\r\nEXEC [CNT].[Sp_AsientoContableSlaveCXC] {Fecha.Year}, {Fecha.Month}, 'CXC', @p_Retorno OUT, @p_Mensaje OUT\r\n\r\nSELECT @p_Retorno AS p_Retorno, @p_Mensaje AS p_Mensaje";
                            Sql3 = $"EXEC CNT.CierreMes_Merge 'CXC', {Fecha.Year}, {Fecha.Month}";
                            col = "Cuenta_Por_Cobrar";
                            break;
                        case "04"://Caja
                            Sql1 = $" DECLARE @p_Retorno INT = 0,\r\n  \t@p_Mensaje NVARCHAR(500)= ''\r\n\r\nEXEC [CNT].[Sp_AsientoContableMasterCAJ] {Fecha.Year}, {Fecha.Month}, 'CAJ', @p_Retorno OUT, @p_Mensaje OUT\r\n\r\nSELECT @p_Retorno AS p_Retorno, @p_Mensaje AS p_Mensaje";
                            Sql2 = $" DECLARE @p_Retorno INT = 0,\r\n  \t@p_Mensaje NVARCHAR(500)= ''\r\n\r\nEXEC [CNT].[Sp_AsientoContableSlaveCAJ] {Fecha.Year}, {Fecha.Month}, 'CAJ', @p_Retorno OUT, @p_Mensaje OUT\r\n\r\nSELECT @p_Retorno AS p_Retorno, @p_Mensaje AS p_Mensaje";
                            Sql3 = $"EXEC CNT.CierreMes_Merge 'CAJ', {Fecha.Year}, {Fecha.Month}";
                            col = "Caja";
                            break;
                        case "05"://Importaciones
                            Sql1 = $" DECLARE @p_Retorno INT = 0,\r\n  \t@p_Mensaje NVARCHAR(500)= ''\r\n\r\nEXEC [CNT].[Sp_AsientoContableMasterLIQ] {Fecha.Year}, {Fecha.Month}, 'LIQ', @p_Retorno OUT, @p_Mensaje OUT\r\n\r\nSELECT @p_Retorno AS p_Retorno, @p_Mensaje AS p_Mensaje";
                            Sql2 = $" DECLARE @p_Retorno INT = 0,\r\n  \t@p_Mensaje NVARCHAR(500)= ''\r\n\r\nEXEC [CNT].[Sp_AsientoContableSlaveLIQ] {Fecha.Year}, {Fecha.Month}, 'LIQ', @p_Retorno OUT, @p_Mensaje OUT\r\n\r\nSELECT @p_Retorno AS p_Retorno, @p_Mensaje AS p_Mensaje";
                            Sql3 = $"EXEC CNT.CierreMes_Merge 'LIQ', {Fecha.Year}, {Fecha.Month}";
                            col = "Importaciones";
                            break;
                        case "06"://Costos de Venta
                            Sql1 = $" DECLARE @p_Retorno INT = 0,\r\n  \t@p_Mensaje NVARCHAR(500)= ''\r\n\r\nEXEC [CNT].[Sp_AsientoContableMasterCOGS] {Fecha.Year}, {Fecha.Month}, 'COGS', @p_Retorno OUT, @p_Mensaje OUT\r\n\r\nSELECT @p_Retorno AS p_Retorno, @p_Mensaje AS p_Mensaje";
                            Sql2 = $" DECLARE @p_Retorno INT = 0,\r\n  \t@p_Mensaje NVARCHAR(500)= ''\r\n\r\nEXEC [CNT].[Sp_AsientoContableSlaveCOGS] {Fecha.Year}, {Fecha.Month}, 'COGS', @p_Retorno OUT, @p_Mensaje OUT\r\n\r\nSELECT @p_Retorno AS p_Retorno, @p_Mensaje AS p_Mensaje";
                            Sql3 = $"EXEC CNT.CierreMes_Merge 'COGS', {Fecha.Year}, {Fecha.Month}";
                            col = "Costos_De_Venta";
                            break;
                    }

                   // Conexion.Database.SetCommandTimeout(TimeSpan.FromMinutes(8));
                    CierreMes Cierre = Conexion.CierreMes.FromSqlRaw(Sql1).ToList().First();
                    Conexion.SaveChanges();

                    CierreMes Cierre2 = Conexion.CierreMes.FromSqlRaw(Sql2).ToList().First();
                    Conexion.SaveChanges();


                    Conexion.Database.ExecuteSqlRaw(Sql3);




                    //Conexion.Database.ExecuteSqlRaw("ENABLE TRIGGER TR_AUDITORIA_CNT_AsientosContablesDetalle ON  CNT.AsientosContablesDetalle;");
                    //Conexion.Database.ExecuteSqlRaw("ENABLE TRIGGER TR_AUDITORIA_CNT_AsientosContables ON  CNT.AsientosContables;");



                    //Conexion.SaveChanges();


                    if (cont == null)
                    {
                        Conexion.Database.ExecuteSqlRaw($"INSERT INTO CNT.CierreProcesado\r\nVALUES(0,0,0,0,0,0,{Fecha.Month},{Fecha.Year}, '{Usuario}', GETDATE())");
                        Conexion.SaveChanges();
                    }

                   Completado =  Conexion.Database.SqlQueryRaw<bool>($"SELECT Completado FROM CNT.CierreProcesado WHERE Mes = {Fecha.Month} AND Anio = {Fecha.Year}").FirstOrDefault();

                    if(Completado)
                    {
                        Conexion.Database.ExecuteSqlRaw($"CNT.SP_MovimientoPeriodo '{string.Format("{0:yyyy-MM-dd}", f1)}', '{string.Format("{0:yyyy-MM-dd}", f2)}', 1");
                        Conexion.SaveChanges();
                    }


                    if (Cierre.p_Retorno == 0 && Cierre2.p_Retorno == 0)
                    {
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





        [Route("api/Contabilidad/CierreMensual/ModuloVSContabilidad")]
        [HttpGet]
        public string ModuloVSContabilidad(int Nivel, string Tabla, string CodBodega, string TipoDoc, string CodConfig, string NoDocumento, DateTime Fecha,  bool esCordoba)
        {
            return V_ModuloVSContabilidad(Nivel, Tabla, CodBodega, TipoDoc, CodConfig, NoDocumento, Fecha, esCordoba);
        }

        private string V_ModuloVSContabilidad(int Nivel, string Tabla, string CodBodega, string TipoDoc, string CodConfig, string NoDocumento, DateTime Fecha, bool esCordoba)
        {
            string json = string.Empty;
            try
            {
                if (Tabla == null) Tabla = string.Empty;
                if (CodBodega == null) CodBodega = string.Empty;
                if (TipoDoc == null) TipoDoc = string.Empty;
                if (CodConfig == null) CodConfig = string.Empty;
                if (NoDocumento == null) NoDocumento = string.Empty;
                using (Conexion)
                {


           

                    List<ModuloVSContabilidad> lst = Conexion.ModuloVSContabilidad.FromSqlRaw($"EXEC CNT.Modulo_VS_Contabilidad {Nivel}, '{Tabla}', '{CodBodega}', '{TipoDoc}', '{CodConfig}', '{NoDocumento}',  {Fecha.Month}, {Fecha.Year}, {(esCordoba ? 1 : 0)}").ToList();
        




                    Cls_Datos datos = new();
                    datos.Nombre = "MODULO VS CONTABILIDAD";
                    datos.d = lst;



                    json = Cls_Mensaje.Tojson(datos, lst.Count, string.Empty, string.Empty, 0);
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
