using Balance_api.Class;
using Balance_api.Contexts;
using Balance_api.Models.Contabilidad;
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

        [Route("api/Contabilidad/TipoComprobante/Get")]
        [HttpGet]
        public string Get()
        {
            return V_Get();
        }

        private string V_Get()
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new();


                    var TComprobantes = (from _q in Conexion.TipoComprobanteRep
                                  select new
                                  {
                                      _q.IdTipoComprobanterpt,
                                      _q.IdSerie,
                                      _q.TipoComprobante
                                  }).ToList();

                    Cls_Datos datos = new();
                    datos.Nombre = "TIPO COMPROBANTES";
                    datos.d = TComprobantes;

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

        [Route("api/Contabilidad/Reporte/Comprobantes")]
        [HttpGet]
        public string Comprobantes(DateTime FechaInicial, string CodBodega, string TipoDocumento, string NoAsiento, int Moneda)
        {
            return V_Comprobantes(FechaInicial, CodBodega, TipoDocumento, NoAsiento, Moneda);
        }

        private string V_Comprobantes(DateTime FechaInicial, string CodBodega, string TipoDocumento, string NoAsiento, int Moneda)
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
                    sqlDataSource.Queries["CNT_SP_ReporteComprobanteGenerales"].Parameters["@CODBODEGA"].Value = CodBodega;
                    sqlDataSource.Queries["CNT_SP_ReporteComprobanteGenerales"].Parameters["@TIPODOCUMENTO"].Value = TipoDocumento;
                    sqlDataSource.Queries["CNT_SP_ReporteComprobanteGenerales"].Parameters["@NOASIENTO"].Value = NoAsiento;
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
            return V_EstadoCambioPatrimonio(FechaInicial, FechaFinal);
        }

        private string V_EstadoCambioPatrimonio(DateTime FechaInicial, DateTime FechaFinal)
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
