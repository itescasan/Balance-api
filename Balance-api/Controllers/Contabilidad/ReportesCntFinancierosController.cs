using Balance_api.Class;
using Balance_api.Contexts;
using Balance_api.Models.Contabilidad;
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


        [Route("api/Contabilidad/AsientosContables/Get")]
        [HttpGet]
        public string AsientosContables(DateTime Fecha)
        {
            return V_AsientosContables(Fecha);
        }

        private string V_AsientosContables(DateTime Fecha)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new();


                    var TAsientosContables = (from _q in Conexion.AsientosContables
                                              where _q.Fecha.Month == Fecha.Month
                                              where _q.Fecha.Year == Fecha.Year
                                              where _q.Estado != "ANULADO"
                                              select new
                                              {
                                                  _q.IdAsiento,
                                                  _q.NoAsiento,
                                                  _q.Fecha,
                                                  _q.Concepto,
                                                  Concep = string.Concat(_q.NoAsiento, ", ", _q.Fecha.ToShortDateString(), " - ", _q.Concepto)
                                              }).ToList();

                    Cls_Datos datos = new();
                    datos.Nombre = "Asientos Contables";
                    datos.d = TAsientosContables;

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


        [Route("api/Contabilidad/CuentasContables/Get")]
        [HttpGet]
        public string CuentasContables()
        {
            return V_CuentasContables();
        }

        private string V_CuentasContables()
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new();


                    var TCuentasContables = (from _q in Conexion.CatalogoCuenta
                                             select new
                                             {
                                                 _q.CuentaContable,
                                                 Nombre = string.Concat(_q.CuentaContable, "-", _q.NombreCuenta)
                                             }).ToList();

                    Cls_Datos datos = new();
                    datos.Nombre = "CATALOGO DE CUENTAS";
                    datos.d = TCuentasContables;

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


        [Route("api/Contabilidad/CentroCosto/Get")]
        [HttpGet]
        public string CentroCosto()
        {
            return V_CentroCosto();
        }

        private string V_CentroCosto()
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new();


                    var TCentroCosto = (from _q in Conexion.CentroCostos
                                             select new
                                             {
                                                 _q.IdCentroCosto,
                                                 _q.Codigo,
                                                 _q.CentroCosto
                                             }).ToList();

                    Cls_Datos datos = new();
                    datos.Nombre = "CENTRO COSTO";
                    datos.d = TCentroCosto;

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

                    rpt.xrlVariables.Text = NoAsiento;

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


        [Route("api/Contabilidad/Reporte/Comprobantes2")]
        [HttpGet]
        public string Comprobantes2(string NoAsiento, DateTime FechaInicial, string Concepto, int Moneda)
        {
            return V_Comprobantes2(NoAsiento, FechaInicial, Concepto, Moneda);
        }

        private string V_Comprobantes2(string NoAsiento, DateTime FechaInicial, string Concepto, int Moneda)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    Cls_Datos Datos = new();

                    xrpComprobantes2 rpt = new xrpComprobantes2();

                    SqlDataSource sqlDataSource = (SqlDataSource)rpt.DataSource;

                    sqlDataSource.Queries["CNT_SP_ReporteComprobanteGenerales2"].Parameters["@NOASIENTO"].Value = NoAsiento;
                    sqlDataSource.Queries["CNT_SP_ReporteComprobanteGenerales2"].Parameters["@FECHA"].Value = FechaInicial;
                    sqlDataSource.Queries["CNT_SP_ReporteComprobanteGenerales2"].Parameters["@CONCEPTO"].Value = Concepto;                    
                    sqlDataSource.Queries["CNT_SP_ReporteComprobanteGenerales2"].Parameters["@MONEDA"].Value = Moneda;

                    rpt.xrlVariables.Text = string.Concat(NoAsiento," - ", Concepto," - ", FechaInicial.ToShortDateString());

                    MemoryStream stream = new MemoryStream();
                    rpt.ExportToPdf(stream, null);
                    stream.Seek(0, SeekOrigin.Begin);

                    Datos.d = stream.ToArray();
                    Datos.Nombre = "Comprobantes2";

                    json = Cls_Mensaje.Tojson(Datos, 1, string.Empty, string.Empty, 0);
                }

            }
            catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;
        }


        [Route("api/Contabilidad/Reporte/DiferenciasCXCvsCONT")]
        [HttpGet]
        public string DiferenciasCXCvsCONT(DateTime FechaInicial, int Moneda)
        {
            return V_DiferenciasCXCvsCONT(FechaInicial, Moneda);
        }

        private string V_DiferenciasCXCvsCONT(DateTime FechaInicial, int Moneda)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    Cls_Datos Datos = new();

                    xrpDiferenciasCXCvsCONT rpt = new xrpDiferenciasCXCvsCONT();

                    SqlDataSource sqlDataSource = (SqlDataSource)rpt.DataSource;
                                        
                    sqlDataSource.Queries["CNT_SP_DiferenciasCXCvsCONT"].Parameters["@FECHAINICIAL"].Value = FechaInicial;                    
                    sqlDataSource.Queries["CNT_SP_DiferenciasCXCvsCONT"].Parameters["@MONEDA"].Value = Moneda;

                    string mnd = (Moneda == 1) ? "Córdobas" : "Dólares";

                    rpt.xrlVariables.Text = "Cortado al "+ FechaInicial.ToShortDateString() + " en " + mnd;

                    MemoryStream stream = new MemoryStream();
                    rpt.ExportToPdf(stream, null);
                    stream.Seek(0, SeekOrigin.Begin);

                    Datos.d = stream.ToArray();
                    Datos.Nombre = "Diferencias CXC vs CONT";

                    json = Cls_Mensaje.Tojson(Datos, 1, string.Empty, string.Empty, 0);
                }

            }
            catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;
        }


        [Route("api/Contabilidad/Reporte/ReporteVentasBolsaAgropecuaria")]
        [HttpGet]
        public string ReporteVentasBolsaAgropecuaria(DateTime FechaInicial, int Moneda)
        {
            return V_ReporteVentasBolsaAgropecuaria(FechaInicial, Moneda);
        }

        private string V_ReporteVentasBolsaAgropecuaria(DateTime FechaInicial, int Moneda)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    Cls_Datos Datos = new();

                    xrpVentasBolsaAgropecuaria rpt = new xrpVentasBolsaAgropecuaria();

                    SqlDataSource sqlDataSource = (SqlDataSource)rpt.DataSource;

                    sqlDataSource.Queries["CNT_SP_ReporteVentasBolsaAgropecuaria"].Parameters["@FECHAINICIAL"].Value = FechaInicial;
                    sqlDataSource.Queries["CNT_SP_ReporteVentasBolsaAgropecuaria"].Parameters["@MONEDA"].Value = Moneda;

                    string mnd = (Moneda == 1) ? "CORDOBAS" : "DOLARES";

                    rpt.xrlVariables.Text = "REPORTE BOLSA AGROPECUARIA EN "+ mnd;                    
                    rpt.xrlFecha.Text = String.Format("{0:MMMM - yyyy}", FechaInicial).ToUpper();

                    MemoryStream stream = new MemoryStream();
                    rpt.ExportToPdf(stream, null);
                    stream.Seek(0, SeekOrigin.Begin);

                    Datos.d = stream.ToArray();
                    Datos.Nombre = "Ventas Bolsa Agropecuaria";

                    json = Cls_Mensaje.Tojson(Datos, 1, string.Empty, string.Empty, 0);
                }

            }
            catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;
        }


        [Route("api/Contabilidad/Reporte/AuxiliaresContables")]
        [HttpGet]
        public string AuxiliaresContables(DateTime FechaInicial, DateTime FechaFinal, string CCInicial, string CCFinal, string CentroCosto, int Moneda)
        {
            return V_AuxiliaresContables(FechaInicial, FechaFinal, CCInicial, CCFinal, CentroCosto, Moneda);
        }

        private string V_AuxiliaresContables(DateTime FechaInicial, DateTime FechaFinal, string CCInicial, string CCFinal, string CentroCosto, int Moneda)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                { 
                    Cls_Datos Datos = new();

                    xrpAuxiliaresContables rpt = new xrpAuxiliaresContables();

                    SqlDataSource sqlDataSource = (SqlDataSource)rpt.DataSource;

                    sqlDataSource.Queries["CNT_SP_ReporteAuxiliaresContables"].Parameters["@FECHAINICIAL"].Value = FechaInicial;
                    sqlDataSource.Queries["CNT_SP_ReporteAuxiliaresContables"].Parameters["@FECHAFINAL"].Value = FechaFinal;
                    sqlDataSource.Queries["CNT_SP_ReporteAuxiliaresContables"].Parameters["@CCINICIAL"].Value = CCInicial;
                    sqlDataSource.Queries["CNT_SP_ReporteAuxiliaresContables"].Parameters["@CCFINAL"].Value = CCFinal;
                    sqlDataSource.Queries["CNT_SP_ReporteAuxiliaresContables"].Parameters["@CENTROCOSTO"].Value = CentroCosto;
                    sqlDataSource.Queries["CNT_SP_ReporteAuxiliaresContables"].Parameters["@MONEDA"].Value = Moneda;
                     
                    string mnd = (Moneda == 1) ? "CORDOBAS" : "DOLARES";
                    rpt.xrlVariables.Text = "TARJETA AUXILIAR EN " + mnd;
                    rpt.xrlFecha.Text = "DEL " + FechaInicial.ToShortDateString() + " AL " + FechaFinal.ToShortDateString();

                    rpt.xrTextoSaldo.Text = "Saldo Inicial al " + FechaInicial.ToShortDateString();

                    MemoryStream stream = new MemoryStream();
                    rpt.ExportToPdf(stream, null);
                    stream.Seek(0, SeekOrigin.Begin);

                    Datos.d = stream.ToArray();
                    Datos.Nombre = "Auxiliares Contables";

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
