using Balance_api.Class;
using Balance_api.Contexts;
using Balance_api.Reporte.Contabilidad;
using DevExpress.DataAccess.DataFederation;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraReports;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Balance_api.Controllers.Contabilidad
{
    public class ReporteContableController : Controller
    {

        private readonly BalanceEntities Conexion;

        public ReporteContableController(BalanceEntities db)
        {
            Conexion = db;
        }



        [Route("api/Contabilidad/Reporte/BalanzaComprobacion")]
        [HttpGet]
        public string BalanzaComprobacion(DateTime FechaInicio, DateTime FechaFinal, int Nivel)
        {
            return V_BalanzaComprobacion(FechaInicio, FechaFinal, Nivel);
        }

        private string V_BalanzaComprobacion(DateTime FechaInicio, DateTime FechaFinal, int Nivel)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    Cls_Datos Datos = new();



                    xrpBalanzaComprobacion rpt = new xrpBalanzaComprobacion();

                    SqlDataSource sqlDataSource = (SqlDataSource)rpt.DataSource;


                    sqlDataSource.Queries["CNT_XRP_Balanza_Comprobacion"].Parameters["@_Fecha_Inicial"].Value = FechaInicio;
                    sqlDataSource.Queries["CNT_XRP_Balanza_Comprobacion"].Parameters["@_Fecha_Final"].Value = FechaFinal;
                    sqlDataSource.Queries["CNT_XRP_Balanza_Comprobacion"].Parameters["@_Nivel"].Value = Nivel;

          

                    MemoryStream stream = new MemoryStream();
                    rpt.ExportToPdf(stream, null);
                    stream.Seek(0, SeekOrigin.Begin);

                    Datos.d = stream.ToArray();
                    Datos.Nombre = "Balanza Comprobacion";



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
