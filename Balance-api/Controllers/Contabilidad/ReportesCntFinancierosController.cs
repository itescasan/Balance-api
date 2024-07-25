using Balance_api.Class;
using Balance_api.Contexts;
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

        [Route("api/Contabilidad/Reporte/Comprobantes")]
        [HttpGet]
        public string Comprobantes(DateTime FechaInicial, DateTime FechaFinal)
        {
            return _Comprobantes(FechaInicial, FechaFinal);
        }

        private string _Comprobantes(DateTime FechaInicial, DateTime FechaFinal)
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
