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
    public class ReporteChequeController : Controller
    {
        private readonly BalanceEntities Conexion;

        public ReporteChequeController(BalanceEntities db)
        {
            Conexion = db;
        }

        [Route("api/Contabilidad/Reporte/Cheque")]
        [HttpGet]
        public string Cheque(string NoAsiento)
        {
            return V_Cheque(NoAsiento);
        }

        private string V_Cheque(string NoAsiento)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    Cls_Datos Datos = new();



                    xrpCheque rpt = new xrpCheque();

                    SqlDataSource sqlDataSource = (SqlDataSource)rpt.DataSource;


                    sqlDataSource.Queries["CNT_XRP_Cheques"].Parameters["@_NoAsiento"].Value = NoAsiento;
                    


                    MemoryStream stream = new MemoryStream();
                    rpt.ExportToPdf(stream, null);
                    stream.Seek(0, SeekOrigin.Begin);

                    Datos.d = stream.ToArray();
                    Datos.Nombre = "Cheque";



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
