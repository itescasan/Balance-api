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
    public class ReporteIngresosCaja : Controller
    {

        private readonly BalanceEntities Conexion;

        public ReporteIngresosCaja(BalanceEntities db)
        {
            Conexion = db;
        }

        [Route("api/Contabilidad/Reporte/IngresosCaja")]
        [HttpGet]
        public string Reembolso(int Id)
        {
            return V_Reembolso(Id);
        }

        private string V_Reembolso(int Id)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    Cls_Datos Datos = new();



                    xrpReembolsoCaja rpt = new xrpReembolsoCaja();

                    SqlDataSource sqlDataSource = (SqlDataSource)rpt.DataSource;


                    sqlDataSource.Queries["rptReporteIngresoCaja"].Parameters["@Id"].Value = Id;



                    MemoryStream stream = new MemoryStream();
                    rpt.ExportToPdf(stream, null);
                    stream.Seek(0, SeekOrigin.Begin);

                    Datos.d = stream.ToArray();
                    Datos.Nombre = "Reembolso";



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
