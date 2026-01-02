using Balance_api.Class;
using Balance_api.Contexts;
using Balance_api.Models.Contabilidad;
using Balance_api.Reporte.Contabilidad;
using DevExpress.DataAccess.Sql;
using Microsoft.AspNetCore.Mvc;

namespace Balance_api.Controllers.Contabilidad
{
    public class RptBalanceSituacionFinancieraController : Controller
    {
        private readonly BalanceEntities Conexion;

        public RptBalanceSituacionFinancieraController(BalanceEntities db)
        {
            Conexion = db;
        }

        [Route("api/Contabilidad/Reporte/BalanceSituacionFinanciera")]
        [HttpGet]
        public string BalanceSituacionFinanciera(int Moneda, DateTime FechaInicial, DateTime FechaFinal, int Nivel, bool Nivel_Ant)
        {
            return V_BalanceSituacionFinanciera(Moneda, FechaInicial, FechaFinal, Nivel, Nivel_Ant);
        }

        private string V_BalanceSituacionFinanciera(int Moneda, DateTime FechaInicial, DateTime FechaFinal, int Nivel, bool Nivel_Ant)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    Cls_Datos Datos = new();

                    xrpBalanceSituacionFinanciera rpt = new xrpBalanceSituacionFinanciera();

                    SqlDataSource sqlDataSource = (SqlDataSource)rpt.DataSource;

                    sqlDataSource.Queries["CNT_SP_BalanceSituacionFinanciera"].Parameters["@MONEDA"].Value = Moneda;
                    sqlDataSource.Queries["CNT_SP_BalanceSituacionFinanciera"].Parameters["@FECHAINICIAL"].Value = FechaInicial;
                    sqlDataSource.Queries["CNT_SP_BalanceSituacionFinanciera"].Parameters["@NIVEL"].Value = Nivel;
                    sqlDataSource.Queries["CNT_SP_BalanceSituacionFinanciera"].Parameters["@NIVEL_ANT"].Value = Nivel_Ant;

                    rpt.xrlFecha.Text = "Al " + FechaFinal.ToShortDateString();
                     
                    string mnd = (Moneda == 1) ? "Córdobas" : "Dólares";

                    rpt.xrlMoneda.Text = "Expresado en " + mnd;

                    MemoryStream stream = new MemoryStream();
                    rpt.ExportToPdf(stream, null);
                    stream.Seek(0, SeekOrigin.Begin);

                    Datos.d = stream.ToArray();
                    Datos.Nombre = "Balance Situacion Financiera";

                    json = Cls_Mensaje.Tojson(Datos, 1, string.Empty, string.Empty, 0);
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
