using Balance_api.Class;
using Balance_api.Contexts;
using Balance_api.Models.Contabilidad;
using Balance_api.Models.Inventario;
using Balance_api.Models.Sistema;
using Balance_api.Reporte.Contabilidad;
using DevExpress.CodeParser;
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
                    DateTime Fecha2 = Fecha.AddMonths(1).AddDays(-Fecha.Day);
                    //DateTime Fecha2 = new DateTime(Fecha.Year, Fecha.Month + 1, 1).AddDays(-1);

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

                    CatalogoCentroCostos? CC = Conexion.CatalogoCentroCostos.FirstOrDefault(f => f.Codigo == CCosto);

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

        [Route("api/Contabilidad/Reporte/LibroDiario")]
        [HttpGet]
        public string LibroDiario(DateTime Fecha, bool EsMonedaLocal, bool Estado)
        {
            return V_LibroDiario(Fecha, EsMonedaLocal,Estado);
        }

        private string V_LibroDiario(DateTime Fecha, bool EsMonedaLocal, bool Estado)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    Cls_Datos Datos = new();

                    Fecha = new DateTime(Fecha.Year, Fecha.Month, 1);
                    //DateTime Fecha2 = new DateTime(Fecha.Year, Fecha.Month + 1, 1).AddDays(-1);
                    DateTime Fecha2 = Fecha.AddMonths(1).AddDays(-Fecha.Day);

                    xrpLibroDiario rpt = new xrpLibroDiario();
                    

                    rpt.Parameters["parameter1"].Value = $"Al {Fecha2.Day} de {string.Format("{0:MMMM}", Fecha)} {Fecha.Year}";
                   
                    SqlDataSource sqlDataSource = (SqlDataSource)rpt.DataSource;

                    sqlDataSource.Queries["CNT_SP_rptLibroDiario"].Parameters["@_Fecha_Inicial"].Value = Fecha;
                    sqlDataSource.Queries["CNT_SP_rptLibroDiario"].Parameters["@_Estado"].Value = Estado;
                    sqlDataSource.Queries["CNT_SP_rptLibroDiario"].Parameters["@_MonedaLocal"].Value = EsMonedaLocal;
                   

                    MemoryStream stream = new MemoryStream();

                    rpt.ExportToPdf(stream, null);
                    stream.Seek(0, SeekOrigin.Begin);

                    Datos.d = stream.ToArray();
                    Datos.Nombre = "LibroDiario";



                    json = Cls_Mensaje.Tojson(Datos, 1, string.Empty, string.Empty, 0);
                }



            }
            catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;
        }

        [Route("api/Contabilidad/Reporte/LibroMayor")]
        [HttpGet]
        public string LibroMayor(DateTime Fecha, bool EsMonedaLocal, bool Estado)
        {
            return V_LibroMayor(Fecha, EsMonedaLocal, Estado);
        }

        private string V_LibroMayor(DateTime Fecha, bool EsMonedaLocal, bool Estado)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    Cls_Datos Datos = new();

                    Fecha = new DateTime(Fecha.Year, Fecha.Month, 1);
                    //DateTime Fecha2 = new DateTime(Fecha.Year, Fecha.Month + 1, 1).AddDays(-1);
                    DateTime Fecha2 = Fecha.AddMonths(1).AddDays(-Fecha.Day);

                    xrpLibroMayor rpt = new xrpLibroMayor();


                    rpt.Parameters["parameter1"].Value = $"Al {Fecha2.Day} de {string.Format("{0:MMMM}", Fecha)} {Fecha.Year}";

                    SqlDataSource sqlDataSource = (SqlDataSource)rpt.DataSource;

                    sqlDataSource.Queries["CNT_SP_rptLibroMayor"].Parameters["@_Fecha_Inicial"].Value = Fecha;
                    sqlDataSource.Queries["CNT_SP_rptLibroMayor"].Parameters["@_MonedaLocal"].Value = EsMonedaLocal;
                    sqlDataSource.Queries["CNT_SP_rptLibroMayor"].Parameters["@_Estado"].Value = Estado;


                    MemoryStream stream = new MemoryStream();

                    rpt.ExportToPdf(stream, null);
                    stream.Seek(0, SeekOrigin.Begin);

                    Datos.d = stream.ToArray();
                    Datos.Nombre = "LibroMayor";



                    json = Cls_Mensaje.Tojson(Datos, 1, string.Empty, string.Empty, 0);
                }



            }
            catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;
        }

        [Route("api/Contabilidad/Reporte/VentasAlcaldia")]
        [HttpGet]
        public string VentasAlcaldia(DateTime Fecha, string Sucursal, string Municipio)
        {
            return V_VentasAlcaldia(Fecha, Sucursal, Municipio);
        }

        private string V_VentasAlcaldia(DateTime Fecha, string Sucursal, string Municipio)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    Cls_Datos Datos = new();

                    Fecha = new DateTime(Fecha.Year, Fecha.Month, 1);
                    //DateTime Fecha2 = new DateTime(Fecha.Year, Fecha.Month + 1, 1).AddDays(-1);
                    DateTime Fecha2 = Fecha.AddMonths(1).AddDays(-Fecha.Day);

                    var Bodega = "";

                    if (Sucursal is not null)
                    {
                        Bodegas? B = Conexion.Bodegas.FirstOrDefault(f => f.Codigo == Sucursal);
                        Bodega = B.Bodega.ToString();
                    }

                  

                    xrpClientesImpuestoAlcaldia rpt = new xrpClientesImpuestoAlcaldia();


                    rpt.Parameters["parameter1"].Value = $"Al {Fecha2.Day} de {string.Format("{0:MMMM}", Fecha)} {Fecha.Year}";

                    SqlDataSource sqlDataSource = (SqlDataSource)rpt.DataSource;

                    sqlDataSource.Queries["CNT_SP_rptClientesImpuestoAlcaldia"].Parameters["@P_Fecha_Inicial"].Value = Fecha;
                    sqlDataSource.Queries["CNT_SP_rptClientesImpuestoAlcaldia"].Parameters["@P_Sucursal"].Value = Bodega == null ? "" : Bodega;
                    sqlDataSource.Queries["CNT_SP_rptClientesImpuestoAlcaldia"].Parameters["@P_Municipio"].Value = Municipio == null ? "" : Municipio;


                    MemoryStream stream = new MemoryStream();

                    rpt.ExportToPdf(stream, null);
                    stream.Seek(0, SeekOrigin.Begin);

                    Datos.d = stream.ToArray();
                    Datos.Nombre = "VentasAlcaldia";



                    json = Cls_Mensaje.Tojson(Datos, 1, string.Empty, string.Empty, 0);
                }



            }
            catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;
        }

        [Route("api/Contabilidad/Reporte/VentasConImpuestos")]
        [HttpGet]
        public string VentasConImpuestos(DateTime FechaI, DateTime FechaF)
        {
            return V_VentasConImpuestos(FechaI,  FechaF);
        }

        private string V_VentasConImpuestos(DateTime FechaI, DateTime FechaF)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    Cls_Datos Datos = new();                    

                    xrpVentasconImpuestos rpt = new xrpVentasconImpuestos();


                    //rpt.Parameters["parameter1"].Value = $"Al {Fecha2.Day} de {string.Format("{0:MMMM}", Fecha)} {Fecha.Year}";

                    SqlDataSource sqlDataSource = (SqlDataSource)rpt.DataSource;

                    sqlDataSource.Queries["FAC_SP_ReporteFinancieroImpuestos"].Parameters["@FECHAINICIAL"].Value = FechaI;
                    sqlDataSource.Queries["FAC_SP_ReporteFinancieroImpuestos"].Parameters["@FECHAFINAL"].Value = FechaF;                   


                    MemoryStream stream = new MemoryStream();

                    rpt.ExportToPdf(stream, null);
                    stream.Seek(0, SeekOrigin.Begin);

                    Datos.d = stream.ToArray();
                    Datos.Nombre = "VentasConImpuestos";



                    json = Cls_Mensaje.Tojson(Datos, 1, string.Empty, string.Empty, 0);
                }



            }
            catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;
        }


        [Route("api/Contabilidad/Reporte/FlujoEfectivo")]
        [HttpGet]
        public string FlujoEfectivo(DateTime Fecha, DateTime FechaF, bool EsMonedaLocal, bool Estado)
        {
            return V_FlujoEfectivo(Fecha,FechaF ,EsMonedaLocal, Estado);
        }

        private string V_FlujoEfectivo(DateTime Fecha, DateTime FechaF,bool EsMonedaLocal, bool Estado)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    Cls_Datos Datos = new();
                    Conexion.Database.SetCommandTimeout(6000);

                    //Fecha = new DateTime(Fecha.Year, Fecha.Month, 1);
                    //DateTime Fecha2 = new DateTime(Fecha.Year, Fecha.Month + 1, 1).AddDays(-1);

                    xrpFlujoEfectivo rpt = new xrpFlujoEfectivo();

                    rpt.Parameters["valorExpresado"].Value = EsMonedaLocal == false ? "Expresado en Dolares" : "Expresado en Cordobas";

                    if (Fecha.ToString("dd-MM") == "01-01" && FechaF.ToString("dd-MM") == "31-12") 
                    {
                        rpt.Parameters["Fecha1"].Value = Fecha.ToString("yyyy");
                        rpt.Parameters["Fecha2"].Value = FechaF.Year - 1; ;
                    }
                    else
                    {
                        rpt.Parameters["Fecha1"].Value = "Del " + Fecha.ToString("dd MMM yyyy") + " Al " + FechaF.ToString("dd MMM yyyy");
                        var FI = Fecha.Date.AddYears(-1);
                        var FF = FechaF.Date.AddYears(-1);
                        rpt.Parameters["Fecha2"].Value = "Del " + FI.ToString("dd MMM yyyy") + " Al " + FF.ToString("dd MMM yyyy");

                    }

                    rpt.Parameters["desdehasta"].Value = "Del " + Fecha.ToString("dd-MM-yyyy") + " Al " + FechaF.ToString("dd-MM-yyyy"); 

                    SqlDataSource sqlDataSource = (SqlDataSource)rpt.DataSource;

                    sqlDataSource.Queries["CNT_SP_rptFlujoEfectivo"].Parameters["@_Fecha_Inicial"].Value = Fecha;
                    sqlDataSource.Queries["CNT_SP_rptFlujoEfectivo"].Parameters["@_Fecha_Final"].Value = FechaF;
                    sqlDataSource.Queries["CNT_SP_rptFlujoEfectivo"].Parameters["@_Estado"].Value = Estado;
                    sqlDataSource.Queries["CNT_SP_rptFlujoEfectivo"].Parameters["@_MonedaLocal"].Value = EsMonedaLocal;

                    
                    


                    MemoryStream stream = new MemoryStream();


                    rpt.ExportToPdf(stream, null);
                    stream.Seek(0, SeekOrigin.Begin);

                    Datos.d = stream.ToArray();
                    Datos.Nombre = "FlujoEfectivo";



                    json = Cls_Mensaje.Tojson(Datos, 1, string.Empty, string.Empty, 0);
                }



            }
             catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;
        }

        [Route("api/Contabilidad/Reporte/ComparativoGastosMensual")]
        [HttpGet]
        public string CompratativoGastosM(DateTime Fecha, bool Estado, bool EsMonedaLocal,string CuentaSucursalA, string CuentaSucursalG)
        {
            return V_CompratativoGastosM(Fecha, Estado, EsMonedaLocal, CuentaSucursalA, CuentaSucursalG);
        }

        private string V_CompratativoGastosM(DateTime Fecha, bool Estado, bool EsMonedaLocal, string CuentaSucursalA, string CuentaSucursalG)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    Cls_Datos Datos = new();

                    //Fecha = new DateTime(Fecha.Year, Fecha.Month, 1);
                    //DateTime Fecha2 = new DateTime(Fecha.Year, Fecha.Month + 1, 1).AddDays(-1);

                    xrpComparativoGastos rpt = new xrpComparativoGastos();

                    rpt.Parameters["valorExpresado"].Value = EsMonedaLocal == false ? "Expresado en Dolares" : "Expresado en Cordobas";

                    

                    rpt.Parameters["desdehasta"].Value = "Al " + Fecha.ToString("dd") + " de " + Fecha.ToString("MMMM") + " " +  Fecha.ToString("yyyy");

                    SqlDataSource sqlDataSource = (SqlDataSource)rpt.DataSource;

                    if (CuentaSucursalA == null)
                    {
                        CuentaSucursalA = "";
                    }

                    sqlDataSource.Queries["CNT_SP_ComparativoGastos"].Parameters["@_Fecha_Inicial"].Value = Fecha;
                    sqlDataSource.Queries["CNT_SP_ComparativoGastos"].Parameters["@P_ESTADO"].Value = Estado;
                    sqlDataSource.Queries["CNT_SP_ComparativoGastos"].Parameters["@_MonedaLocal"].Value = EsMonedaLocal;
                    sqlDataSource.Queries["CNT_SP_ComparativoGastos"].Parameters["@_CUENTA_SUCURSAL"].Value = CuentaSucursalA;
                    sqlDataSource.Queries["CNT_SP_ComparativoGastos"].Parameters["@_CUENTA_SUCURSALG"].Value = CuentaSucursalA;


                    MemoryStream stream = new MemoryStream();

                    rpt.ExportToPdf(stream, null);
                    stream.Seek(0, SeekOrigin.Begin);

                    Datos.d = stream.ToArray();
                    Datos.Nombre = "ComparativoGastos";



                    json = Cls_Mensaje.Tojson(Datos, 1, string.Empty, string.Empty, 0);
                }



            }
            catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;
        }

        [Route("api/Contabilidad/Reporte/BalanceGeneralComparativo")]
        [HttpGet]
        public string BalanceGeneralComparativo(DateTime Fecha, bool Estado, bool EsMonedaLocal)
        {
            return V_BalanceGeneralComparativo(Fecha, Estado, EsMonedaLocal);
        }

        private string V_BalanceGeneralComparativo(DateTime Fecha, bool Estado, bool EsMonedaLocal)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    Cls_Datos Datos = new();

                    //Fecha = new DateTime(Fecha.Year, Fecha.Month, 1);
                    //DateTime Fecha2 = new DateTime(Fecha.Year, Fecha.Month + 1, 1).AddDays(-1);

                    xrpBalanceGeneralComparativo rpt = new xrpBalanceGeneralComparativo();

                    rpt.Parameters["valorExpresado"].Value = EsMonedaLocal == false ? "Expresado en Dolares" : "Expresado en Cordobas";

                    DateTime ultimoDiaDelMes = new DateTime(Fecha.Year, Fecha.Month, DateTime.DaysInMonth(Fecha.Year, Fecha.Month));

                    rpt.Parameters["desdehasta"].Value = "Al " + ultimoDiaDelMes.ToString("dd") + " de " + Fecha.ToString("MMMM") + " " + Fecha.ToString("yyyy");

                    SqlDataSource sqlDataSource = (SqlDataSource)rpt.DataSource;


                    sqlDataSource.Queries["CNT_SP_BalanceGeneralComparativo"].Parameters["@_Fecha_Inicial"].Value = Fecha;
                    sqlDataSource.Queries["CNT_SP_BalanceGeneralComparativo"].Parameters["@P_ESTADO"].Value = Estado;
                    sqlDataSource.Queries["CNT_SP_BalanceGeneralComparativo"].Parameters["@_MonedaLocal"].Value = EsMonedaLocal;
                    


                    MemoryStream stream = new MemoryStream();

                    rpt.ExportToPdf(stream, null);
                    stream.Seek(0, SeekOrigin.Begin);

                    Datos.d = stream.ToArray();
                    Datos.Nombre = "BalanceGeneralComparativo";



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
                    var qCentroCosto = Conexion.CatalogoCentroCostos.ToList();


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

                    var qSucursales = (from _q in Conexion.Bodegas 
                                       join _b in Conexion.BodegaSerie on _q.Codigo equals _b.CodBodega
                                       where _b.EsFact == true
                                       orderby _q.Codigo
                                       select new 
                                       {
                                           _q.Codigo,
                                           _q.Bodega
                                       }
                                       ).ToList();


                    datos = new Cls_Datos();
                    datos.Nombre = "Sucursales";
                    datos.d = qSucursales;
                    lstDatos.Add(datos);

                    var qMunicipios = (from _q in Conexion.VentasClientesAlcaldia
                                       where _q.Estado == "A" 
                                       select new
                                       {                                           
                                           _q.Municipio
                                       }                                       
                                       ).Distinct().ToList();

                    datos = new Cls_Datos();
                    datos.Nombre = "Municipios";
                    datos.d = qMunicipios;
                    lstDatos.Add(datos);

                    var qCuentaGastos = (from _q in Conexion.CuentasComparativoGastos                                        
                                       where _q.Activo == true
                                       orderby _q.CuentaGastosVentas
                                       select new
                                       {
                                           _q.CuentaGastosAdmon,
                                           _q.CuentaGastosVentas,
                                           _q.NombreCuenta
                                       }
                                      ).ToList();


                    datos = new Cls_Datos();
                    datos.Nombre = "CuentasGastos";
                    datos.d = qCuentaGastos;
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

