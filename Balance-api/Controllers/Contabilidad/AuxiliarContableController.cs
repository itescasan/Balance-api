using Balance_api.Class;
using Balance_api.Class.Contabilidad;
using Balance_api.Contexts;
using Balance_api.Controllers.Sistema;
using Balance_api.Models.Contabilidad;
using Balance_api.Models.Sistema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;


namespace Balance_api.Controllers.Contabilidad
{
    public class AuxiliarContableController : Controller
    {

        private readonly BalanceEntities Conexion;

        public AuxiliarContableController(BalanceEntities db)
        {
            Conexion = db;
        }


        [Route("api/Contabilidad/AuxiliarContable/Get")]
        [HttpGet]
        public string Get(DateTime Fecha1, DateTime Fecha2, string CodBodega, string Cuenta)
        {
            return  V_Get(Fecha1, Fecha2, CodBodega, Cuenta);
        }

        private string V_Get(DateTime Fecha1, DateTime Fecha2, string CodBodega, string Cuenta)
        {
            if (CodBodega == null) CodBodega = string.Empty;
            if (Cuenta == null) Cuenta = string.Empty;

            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new();
                    
                     string sQuery = $"DECLARE @Fecha1  DATE = CAST('{string.Format("{0:yyyy-MM-dd}", Fecha1.Date)}' AS DATE)," +
                        $"@Fecha2 DATE =  CAST('{string.Format("{0:yyyy-MM-dd}", Fecha2.Date)}' AS DATE)" +
                        $"EXEC [CNT].[SP_AuxiliarCuenta] '{Cuenta}', '{CodBodega}', '3', @Fecha1, @Fecha2, 1";

        
    
                    var qAuxiliar = Conexion.AuxiliarContable.FromSqlRaw<Cls_AuxiliarContable>(sQuery).ToList();
                                     




                    Cls_Datos datos = new();
                    datos.Nombre = "ASIENTO";
                    datos.d = qAuxiliar;

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



        [Route("api/Contabilidad/AuxiliarContable/GetAsiento")]
        [HttpGet]
        public string GetAsiento(string NoDoc, string Serie)
        {
            return V_GetAsiento(NoDoc, Serie);
        }

        private string V_GetAsiento(string NoDoc, string Serie)
        {
       

            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new();

                    var qAsiento = (from _q in Conexion.AsientosContables
                                    where (_q.NoDocOrigen == null ? _q.NoAsiento : _q.NoDocOrigen) == NoDoc //&& (_q.IdSerieDocOrigen == null ? _q.IdSerie : _q.IdSerieDocOrigen) == Serie
                                    select new
                                    {
                                        _q.IdAsiento,
                                        _q.IdPeriodo,
                                        _q.NoAsiento,
                                        _q.IdSerie,
                                        _q.Fecha,
                                        _q.IdMoneda,
                                        _q.TasaCambio,
                                        _q.Concepto,
                                        _q.NoDocOrigen,
                                        _q.IdSerieDocOrigen,
                                        _q.TipoDocOrigen,
                                        _q.Bodega,
                                        _q.Referencia,
                                        _q.Estado,
                                        _q.TipoAsiento,
                                        _q.Total,
                                        _q.TotalML,
                                        _q.TotalMS,
                                        _q.FechaReg,
                                        _q.UsuarioReg,
                                        AsientosContablesDetalle = _q.AsientosContablesDetalle.Where(w => w.DebitoML + w.CreditoML != 0).OrderBy( o => o.NoLinea).ToList(),

                                    }).Take(1);


                    Cls_Datos datos = new();
                    datos.Nombre = "ASIENTO";
                    datos.d = qAsiento;

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



    }
}
