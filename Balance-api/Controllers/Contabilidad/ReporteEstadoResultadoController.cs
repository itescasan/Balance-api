using Balance_api.Class;
using Balance_api.Contexts;
using Balance_api.Models.Sistema;
using Balance_api.Reporte.Contabilidad;
using DevExpress.DataAccess.DataFederation;
using DevExpress.DataAccess.Sql;
using DevExpress.DataProcessing;
using DevExpress.XtraReports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;


namespace Balance_api.Controllers.Contabilidad
{
    public class ReporteEstadoResultadoController : Controller
    {
        private readonly BalanceEntities Conexion;

        public ReporteEstadoResultadoController(BalanceEntities db)
        {
            Conexion = db;
        }

        [Route("api/Contabilidad/Reporte/EstadoResultado")]
        [HttpGet]
        public string EstadoResultado(DateTime Fecha, bool Estado, bool Moneda)
        {
            return V_EstadoResultado(Fecha, Estado, Moneda);
        }

        private string V_EstadoResultado(DateTime Fecha, bool Estado, bool Moneda)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    Cls_Datos Datos = new();

                    Fecha = new DateTime(Fecha.Year, Fecha.Month, 1);
                    DateTime Fecha2 = new DateTime(Fecha.Year, Fecha.Month + 1, 1).AddDays(-1);

                    xrpEstadoResultado rpt = new xrpEstadoResultado();

                    

                    rpt.Parameters["parameter1"].Value = $"Al {Fecha2.Day} de {string.Format("{0:MMMM}", Fecha)} {Fecha.Year}";

                    SqlDataSource sqlDataSource = (SqlDataSource)rpt.DataSource;

                    
                    sqlDataSource.Queries["CNT_SP_EstadoResultado"].Parameters["@P_ES_ML"].Value = Moneda;
                    sqlDataSource.Queries["CNT_SP_EstadoResultado"].Parameters["@P_FECHA_1"].Value = Fecha;
                    sqlDataSource.Queries["CNT_SP_EstadoResultado"].Parameters["@P_ESTADO"].Value = Estado;
                    sqlDataSource.Queries["CNT_SP_EstadoResultado"].Parameters["@P_CUENTA"].Value = "";


                    MemoryStream stream = new MemoryStream();
                    
                    rpt.ExportToPdf(stream, null);
                    stream.Seek(0, SeekOrigin.Begin);

                    Datos.d = stream.ToArray();
                    Datos.Nombre = "EstadoResultado";



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
