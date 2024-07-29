using Balance_api.Class;
using Balance_api.Contexts;
using Balance_api.Models.Contabilidad;
using Balance_api.Models.Sistema;
using Balance_api.Reporte.Contabilidad;
using DevExpress.DataAccess.DataFederation;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraReports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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
        public string BalanzaComprobacion(DateTime FechaInicio, DateTime FechaFinal, int Nivel, bool EsMonedaLocal)
        {
            return V_BalanzaComprobacion(FechaInicio, FechaFinal, Nivel, EsMonedaLocal);
        }

        private string V_BalanzaComprobacion(DateTime FechaInicio, DateTime FechaFinal, int Nivel, bool EsMonedaLocal)
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
                    sqlDataSource.Queries["CNT_XRP_Balanza_Comprobacion"].Parameters["@_MonedaLocal"].Value = EsMonedaLocal;


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




        [Route("api/Contabilidad/Reporte/BalanceGeneral")]
        [HttpGet]
        public string BalanceGeneral(DateTime Fecha, bool EsMonedaLocal)
        {
            return V_BalanceGeneral(Fecha, EsMonedaLocal);
        }

        private string V_BalanceGeneral(DateTime Fecha, bool EsMonedaLocal)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    Cls_Datos Datos = new();

                    Fecha = new DateTime(Fecha.Year, Fecha.Month, 1);
                    DateTime Fecha2 = Fecha.AddMonths(1).AddDays(-1);

                    string IdMoneda = Conexion.Database.SqlQueryRaw<string>($"SELECT TOP 1 {(EsMonedaLocal ? "MonedaLocal" : "MonedaExtranjera")} FROM SIS.Parametros").ToList().First();
                    Monedas M = Conexion.Monedas.Find(IdMoneda)!;





                    xrpBalanceGeneral rpt = new xrpBalanceGeneral();
                    rpt.Parameters["P_Titulo"].Value = $"Correspondiente al mes de {string.Format("{0:MMMM}", Fecha)} de {Fecha.Year}";
                    rpt.Parameters["P_Moneda"].Value = M.Moneda;

                    SqlDataSource SqlDataSource = (SqlDataSource)rpt.DataSource;

                    SqlDataSource.Queries["CNT_SP_BalanceGeneral"].Parameters["@P_FECHA_1"].Value = Fecha;
                    SqlDataSource.Queries["CNT_SP_BalanceGeneral"].Parameters["@P_FECHA_2"].Value = Fecha2;
                    SqlDataSource.Queries["CNT_SP_BalanceGeneral"].Parameters["@P_ES_ML"].Value = EsMonedaLocal;
                    SqlDataSource.Queries["CNT_SP_BalanceGeneral"].Parameters["@P_GrupoInicio"].Value = 1;
                    SqlDataSource.Queries["CNT_SP_BalanceGeneral"].Parameters["@P_GrupoFin"].Value = 9;




                    SqlDataSource SqlDataSource2 = (SqlDataSource)rpt.xrSubreport1.ReportSource.DataSource;

                    SqlDataSource2.Queries["CNT_SP_BalanceGeneral"].Parameters["@P_FECHA_1"].Value = Fecha;
                    SqlDataSource2.Queries["CNT_SP_BalanceGeneral"].Parameters["@P_FECHA_2"].Value = Fecha2;
                    SqlDataSource2.Queries["CNT_SP_BalanceGeneral"].Parameters["@P_ES_ML"].Value = EsMonedaLocal;
                    SqlDataSource2.Queries["CNT_SP_BalanceGeneral"].Parameters["@P_GrupoInicio"].Value = 1;
                    SqlDataSource2.Queries["CNT_SP_BalanceGeneral"].Parameters["@P_GrupoFin"].Value = 4;

                    SqlDataSource sqlDataSource3 = (SqlDataSource)rpt.xrSubreport2.ReportSource.DataSource;

                    sqlDataSource3.Queries["CNT_SP_BalanceGeneral"].Parameters["@P_FECHA_1"].Value = Fecha;
                    sqlDataSource3.Queries["CNT_SP_BalanceGeneral"].Parameters["@P_FECHA_2"].Value = Fecha2;
                    sqlDataSource3.Queries["CNT_SP_BalanceGeneral"].Parameters["@P_ES_ML"].Value = EsMonedaLocal;
                    sqlDataSource3.Queries["CNT_SP_BalanceGeneral"].Parameters["@P_GrupoInicio"].Value = 5;
                    sqlDataSource3.Queries["CNT_SP_BalanceGeneral"].Parameters["@P_GrupoFin"].Value = 9;




                    MemoryStream stream = new MemoryStream();
                    rpt.ExportToPdf(stream, null);
                    stream.Seek(0, SeekOrigin.Begin);



                    Datos.d = stream.ToArray();
                    Datos.Nombre = "Balance General";




                    json = Cls_Mensaje.Tojson(Datos, 1, string.Empty, string.Empty, 0);
                }



            }
            catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;
        }


        [Route("api/Contabilidad/Reporte/EstadoResultado")]
        [HttpGet]
        public string EstadoResultado(DateTime Fecha, bool Estado, bool EsMonedaLocal,string Sucursal,string CCosto)
        {
            return V_EstadoResultado(Fecha, Estado, EsMonedaLocal, Sucursal, CCosto);
        }

        private string V_EstadoResultado(DateTime Fecha, bool Estado, bool EsMonedaLocal, string Sucursal, string CCosto)
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

                    sqlDataSource.Queries["CNT_SP_EstadoResultado"].Parameters["@_Fecha_Inicial"].Value = Fecha;
                    sqlDataSource.Queries["CNT_SP_EstadoResultado"].Parameters["@P_ESTADO"].Value = Estado;
                    sqlDataSource.Queries["CNT_SP_EstadoResultado"].Parameters["@_MonedaLocal"].Value = EsMonedaLocal;
                    sqlDataSource.Queries["CNT_SP_EstadoResultado"].Parameters["@_SUCURSAL"].Value = Sucursal == null ? "" : Sucursal;
                    sqlDataSource.Queries["CNT_SP_EstadoResultado"].Parameters["@_CCosto"].Value = CCosto == null ? "" : CCosto;
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



        [Route("api/Contabilidad/Reporte/GastoCC")]
        [HttpGet]
        public string GastoCC(DateTime Fecha, bool EsMonedaLocal, string CCosto)
        {
            return V_GastoCC(Fecha, EsMonedaLocal, CCosto);
        }

        private string V_GastoCC(DateTime Fecha, bool EsMonedaLocal, string CCosto)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    Cls_Datos Datos = new();

                    Fecha = new DateTime(Fecha.Year, Fecha.Month, 1);
                    DateTime Fecha2 = new DateTime(Fecha.Year, Fecha.Month + 1, 1).AddDays(-1);

                    xrpComparativoGastosMensual rpt = new xrpComparativoGastosMensual();

                    CentroCostos? CC = Conexion.CentroCostos.FirstOrDefault(f => f.Codigo == CCosto);

                    rpt.Parameters["parameter1"].Value = $"Al {Fecha2.Day} de {string.Format("{0:MMMM}", Fecha)} {Fecha.Year}";
                    rpt.Parameters["parameter2"].Value = CC?.CentroCosto.ToString();

                    SqlDataSource sqlDataSource = (SqlDataSource)rpt.DataSource;

                    sqlDataSource.Queries["CNT_SP_ComparativoGastosAcumuladosMes"].Parameters["@_Fecha_Inicial"].Value = Fecha;                    
                    sqlDataSource.Queries["CNT_SP_ComparativoGastosAcumuladosMes"].Parameters["@_MonedaLocal"].Value = EsMonedaLocal;                   
                    sqlDataSource.Queries["CNT_SP_ComparativoGastosAcumuladosMes"].Parameters["@_CCosto"].Value = CCosto == null ? "" : CCosto;                    


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


        [Route("api/Contabilidad/Reporte/Datos")]
        [HttpGet]
        public string Datos()
        {
            return V_Datos();
        }

        private string V_Datos()
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new();
                    Cls_Datos datos = new();
                    var qCentroCosto = Conexion.CentroCostos.ToList();


                    datos = new();
                    datos.Nombre = "CENTRO COSTO";
                    datos.d = qCentroCosto;
                    lstDatos.Add(datos);



                    var qCuentasC = (from _q in Conexion.CatalogoCuenta
                                     join _i in Conexion.InformesContables on _q.CuentaContable equals _i.Cuenta
                                     where _i.IdTipoInforme == 7 && _i.IdSubGrupo == 8 && _i.Cuenta.Contains("4101-01")
                                     orderby _i.Bodega
                                     select new
                                     {
                                         CuentaContable = _i.Bodega,
                                         NombreCuenta = String.Concat(_i.Bodega + " " + _q.NombreCuenta)

                                     }).ToList();

                    datos = new Cls_Datos();
                    datos.Nombre = "CUENTAS CAJA";
                    datos.d = qCuentasC;
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

