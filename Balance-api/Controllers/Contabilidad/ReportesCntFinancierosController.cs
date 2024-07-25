using Balance_api.Class;
using Balance_api.Contexts;
using Balance_api.Models.Sistema;
using Balance_api.Reporte.Contabilidad;
using DevExpress.DataAccess.Sql;
using Microsoft.AspNetCore.Mvc;

namespace Balance_api.Controllers.Contabilidad
{
    public class ReportesCntFinancierosController : Controller
    {
        private readonly BalanceEntities Conexion;

        public ReportesCntFinancierosController(BalanceEntities db)
        {
            Conexion = db;
        }        

        [Route("api/Contabilidad/Reporte/Comprobantes")]
        [HttpGet]
        public string Comprobantes(DateTime FechaInicial, DateTime FechaFinal, string CodBodega, string TipoDocumento, string IdSerie, int Moneda)
        {
            return _Comprobantes(FechaInicial, FechaFinal, CodBodega, TipoDocumento, IdSerie, Moneda);
        }

        private string _Comprobantes(DateTime FechaInicial, DateTime FechaFinal, string CodBodega, string TipoDocumento, string IdSerie, int Moneda)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    Cls_Datos Datos = new();

                    xrpComprobantes rpt = new xrpComprobantes();

                    SqlDataSource sqlDataSource = (SqlDataSource)rpt.DataSource;
                    
                    sqlDataSource.Queries["CNT_SP_ReporteComprobanteGenerales"].Parameters["@FECHAINICIAL"].Value = FechaInicial;
                    sqlDataSource.Queries["CNT_SP_ReporteComprobanteGenerales"].Parameters["@FECHAFINAL"].Value = FechaFinal;
                    sqlDataSource.Queries["CNT_SP_ReporteComprobanteGenerales"].Parameters["@CODBODEGA"].Value = CodBodega;
                    sqlDataSource.Queries["CNT_SP_ReporteComprobanteGenerales"].Parameters["@TIPODOCUMENTO"].Value = TipoDocumento;
                    sqlDataSource.Queries["CNT_SP_ReporteComprobanteGenerales"].Parameters["@IDSERIE"].Value = IdSerie;
                    sqlDataSource.Queries["CNT_SP_ReporteComprobanteGenerales"].Parameters["@MONEDA"].Value = Moneda;

                    MemoryStream stream = new MemoryStream();
                    rpt.ExportToPdf(stream, null);
                    stream.Seek(0, SeekOrigin.Begin);

                    Datos.d = stream.ToArray();
                    Datos.Nombre = "Comprobantes";

                    json = Cls_Mensaje.Tojson(Datos, 1, string.Empty, string.Empty, 0);
                }

            }
            catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;
        }

        [Route("api/Contabilidad/Reporte/EstadoCambioPatrimonio")]
        [HttpGet]
        public string EstadoCambioPatrimonio(DateTime FechaInicial, DateTime FechaFinal)
        {
            return _EstadoCambioPatrimonio(FechaInicial, FechaFinal);
        }

        private string _EstadoCambioPatrimonio(DateTime FechaInicial, DateTime FechaFinal)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    Cls_Datos Datos = new();



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
