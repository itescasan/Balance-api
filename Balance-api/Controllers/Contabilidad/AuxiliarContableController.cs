using Balance_api.Class;
using Balance_api.Class.Contabilidad;
using Balance_api.Contexts;
using Balance_api.Controllers.Sistema;
using Balance_api.Models.Contabilidad;
using Balance_api.Models.Inventario;
using Balance_api.Models.Sistema;
using Balance_api.Reporte.Contabilidad;
using DevExpress.DataAccess.Sql;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

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

                    var bo = Conexion.Bodegas.FirstOrDefault(f => f.Codigo == CodBodega);

                   
                    xrpAuxiliar rpt = new xrpAuxiliar();
                    rpt.Parameters["P_Fecha1"].Value = Fecha1;
                    rpt.Parameters["P_Fecha2"].Value = Fecha2;
                    rpt.Parameters["P_Cuenta"].Value = Cuenta;
                    rpt.Parameters["P_Bodega"].Value = (bo == null ? string.Empty: string.Concat(bo.Codigo, " - ", bo.Bodega));



                    SqlDataSource sqlDataSource = (SqlDataSource)rpt.DataSource;


                    sqlDataSource.Queries["CNT_SP_AuxiliarCuenta"].Parameters["@CUENTA"].Value = Cuenta;
                    sqlDataSource.Queries["CNT_SP_AuxiliarCuenta"].Parameters["@BODEGA"].Value = CodBodega;
                    sqlDataSource.Queries["CNT_SP_AuxiliarCuenta"].Parameters["@FECHA_1"].Value = Fecha1;
                    sqlDataSource.Queries["CNT_SP_AuxiliarCuenta"].Parameters["@FECHA_2"].Value = Fecha2;

         
                    MemoryStream stream = new MemoryStream();

                    rpt.ExportToPdf(stream, null);
                    stream.Seek(0, SeekOrigin.Begin);


                


                    //DevExpress.DataAccess.Sql.DataApi.ITable table = sqlDataSource.Result["CNT_SP_AuxiliarCuenta"];

                    var qAuxiliar = (from _q in sqlDataSource.Result["CNT_SP_AuxiliarCuenta"]
                              select new Cls_AuxiliarContable()
                              {
                                  Fecha = Convert.ToDateTime(_q["Fecha"]),
                                  Serie = _q["Serie"].ToString(),
                                  NoDoc = _q["NoDoc"].ToString(),
                                  Cuenta = _q["Cuenta"].ToString(),
                                  Concepto = _q["Concepto"].ToString(),
                                  Referencia = _q["Referencia"].ToString(),
                                  DEBE_ML = Convert.ToDecimal(_q["DEBE_ML"]),
                                  HABER_ML = Convert.ToDecimal(_q["HABER_ML"]),
                                  Saldo_ML = Convert.ToDecimal(_q["Saldo_ML"]),
                                  DEBE_MS = Convert.ToDecimal(_q["DEBE_MS"]),
                                  HABER_MS = Convert.ToDecimal(_q["HABER_MS"]),
                                  Saldo_MS = Convert.ToDecimal(_q["Saldo_MS"]),
                                  Cuenta_Padre = _q["Cuenta_Padre"].ToString(),
                                  Editar = Convert.ToInt32(_q["Editar"]),
                                  Linea = Convert.ToInt32(_q["Linea"]),
                                 
                              }).ToList();

                    /*
                    foreach (DevExpress.DataAccess.Sql.DataApi.IRow row in table)
                    {
                        object value = row["Fecha"];
                    }*/




                    /*

                               string sQuery = $"DECLARE @Fecha1  DATE = CAST('{string.Format("{0:yyyy-MM-dd}", Fecha1.Date)}' AS DATE)," +
                                  $"@Fecha2 DATE =  CAST('{string.Format("{0:yyyy-MM-dd}", Fecha2.Date)}' AS DATE)" +
                                  $"EXEC [CNT].[SP_AuxiliarCuenta] '{Cuenta}', '{CodBodega}', @Fecha1, @Fecha2";




                               var qAuxiliar = Conexion.AuxiliarContable.FromSqlRaw<Cls_AuxiliarContable>(sQuery).ToList();

                               */


                    List<Cls_Datos> lstDatos = new();

                    Cls_Datos datos = new();
                    datos.Nombre = "AUXILIAR";
                    datos.d = qAuxiliar;
                    lstDatos.Add(datos);




                    datos = new();
                    datos.d = stream.ToArray();
                    datos.Nombre = "xrpAuxiliar";
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
