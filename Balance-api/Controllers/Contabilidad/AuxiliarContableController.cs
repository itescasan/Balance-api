using Balance_api.Class;
using Balance_api.Class.Contabilidad;
using Balance_api.Contexts;
using Balance_api.Controllers.Sistema;
using Balance_api.Models.Contabilidad;
using Balance_api.Models.Inventario;
using Balance_api.Models.Sistema;
using Balance_api.Reporte.Contabilidad;
using DevExpress.DataAccess.Excel;
using DevExpress.DataAccess.Sql;
using DevExpress.DataAccess.Sql.DataApi;
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
                    CatalogoCuenta? ct = Conexion.CatalogoCuenta.FirstOrDefault(f => f.CuentaContable == Cuenta);



                  var qAuxiliar =  Conexion.Set<Cls_AuxiliarContable>().FromSqlRaw($"EXEC [CNT].[SP_AuxiliarCuenta] '{Cuenta}', '{CodBodega}', '{string.Format("{0:yyyy-MM-dd}", Fecha1)}','{string.Format("{0:yyyy-MM-dd}", Fecha2)}'").ToList();



                    //var qAuxiliar = (from _q in Conexion.Database.SqlQueryRaw<>("CNT_SP_AuxiliarCuenta")
                    //


                    List<Cls_Datos> lstDatos = new();

                    Cls_Datos datos = new();
                    datos.Nombre = "AUXILIAR";
                    datos.d = qAuxiliar;
                    lstDatos.Add(datos);


                    datos = new();
                    datos.d = Cuenta;
                    datos.Nombre = "CUENTA";
                    lstDatos.Add(datos);


                    datos = new();
                    datos.d = ct == null ? string.Empty : ct.ClaseCuenta;
                    datos.Nombre = "CLASE";
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



        [Route("api/Contabilidad/AuxiliarContable/GetReporte")]
        [HttpGet]
        public string GetReporte(DateTime Fecha1, DateTime Fecha2, string CodBodega, string Cuenta, string Tipo)
        {
            return V_GetReporte(Fecha1, Fecha2, CodBodega, Cuenta, Tipo);
        }

        private string V_GetReporte(DateTime Fecha1, DateTime Fecha2, string CodBodega, string Cuenta, string Tipo)
        {
            if (CodBodega == null) CodBodega = string.Empty;
            if (Cuenta == null) Cuenta = string.Empty;

            string json = string.Empty;
            try
            {
                using (Conexion)
                {

                    var bo = Conexion.Bodegas.FirstOrDefault(f => f.Codigo == CodBodega);
                    CatalogoCuenta? ct = Conexion.CatalogoCuenta.FirstOrDefault(f => f.CuentaContable == Cuenta);


                    xrpAuxiliar rpt = new xrpAuxiliar();


                    rpt.Parameters["P_Fecha1"].Value = Fecha1;
                    rpt.Parameters["P_Fecha2"].Value = Fecha2;
                    rpt.Parameters["P_Cuenta"].Value = Cuenta;
                    rpt.Parameters["P_Bodega"].Value = (bo == null ? string.Empty : string.Concat(bo.Codigo, " - ", bo.Bodega));


                    SqlDataSource sqlDataSource = (SqlDataSource)rpt.DataSource;




                    sqlDataSource.Queries["CNT_SP_AuxiliarCuenta"].Parameters["@CUENTA"].Value = Cuenta;
                    sqlDataSource.Queries["CNT_SP_AuxiliarCuenta"].Parameters["@BODEGA"].Value = CodBodega;
                    sqlDataSource.Queries["CNT_SP_AuxiliarCuenta"].Parameters["@FECHA_1"].Value = Fecha1;
                    sqlDataSource.Queries["CNT_SP_AuxiliarCuenta"].Parameters["@FECHA_2"].Value = Fecha2;







                    List<Cls_Datos> lstDatos = new();

                    MemoryStream stream = new MemoryStream();
                    Cls_Datos datos = new();

                    if (Tipo == "PDF")
                    {
                        rpt.ExportToPdf(stream, null);
                        stream.Seek(0, SeekOrigin.Begin);

                       
                    }
                    else
                    {
                        rpt.ExportToXlsx(stream, null);
                        stream.Seek(0, SeekOrigin.Begin);

                   
                    }


                    datos = new();
                    datos.d =  stream.ToArray();
                    datos.Nombre = "xrpAuxiliar";
                    lstDatos.Add(datos);





                    var qAuxiliar = (from _q in sqlDataSource.Result["CNT_SP_AuxiliarCuenta"]
                                     orderby _q["Linea"]
                                     select new Cls_AuxiliarContable()
                                     {
                                         IdAsiento = Convert.ToInt32(_q["IdAsiento"]),
                                         Modulo = Convert.ToString(_q["Modulo"])!,
                                         Fecha = Convert.ToDateTime(_q["Fecha"]),
                                         Serie = _q["Serie"].ToString(),
                                         NoDoc = _q["NoDoc"].ToString(),
                                         Cuenta = _q["Cuenta"].ToString(),
                                         NombreCuenta = _q["NombreCuenta"].ToString(),
                                         Concepto = _q["Concepto"].ToString(),
                                         Referencia = _q["Referencia"].ToString(),
                                         DEBE_ML = Convert.ToDecimal(_q["DEBE_ML"]),
                                         HABER_ML = Convert.ToDecimal(_q["HABER_ML"]),
                                         Saldo_ML = Convert.ToDecimal(_q["Saldo_ML"]),
                                         DEBE_MS = Convert.ToDecimal(_q["DEBE_MS"]),
                                         HABER_MS = Convert.ToDecimal(_q["HABER_MS"]),
                                         Saldo_MS = Convert.ToDecimal(_q["Saldo_MS"]),
                                         TipoDocumento = _q["TipoDocumento"].ToString(),
                                         Cuenta_Padre = _q["Cuenta_Padre"].ToString(),
                                         Bodega = _q["Bodega"].ToString(),
                                         Editar = Convert.ToInt32(_q["Editar"]),
                                         Linea = Convert.ToInt32(_q["Linea"])

                                     }).ToList();





                    if (ct != null)
                    {
                        if (ct.ClaseCuenta == "D")
                        {


                            var qConsolidado = (from _q in qAuxiliar
                                                group _q by new { _q.Modulo, _q.Fecha } into grupo
                                                select grupo).Select((x, i) => new Cls_AuxiliarContable()
                                                {
                                                    IdAsiento = 0,
                                                    Modulo = x.Key.Modulo,
                                                    Fecha = x.Key.Fecha,
                                                    Serie = string.Empty,
                                                    NoDoc = string.Empty,
                                                    Cuenta = x.First().Serie == "INI" ? "SALDO" : ct.CuentaContable,
                                                    NombreCuenta = x.First().Serie == "INI" ? "" : ct.NombreCuenta,
                                                    Concepto = x.First().Serie == "INI" ? "INICIAL" : ct.NombreCuenta,
                                                    Referencia = string.Empty,
                                                    DEBE_ML = x.Sum(s => s.DEBE_ML),
                                                    HABER_ML = x.Sum(s => s.HABER_ML),
                                                    Saldo_ML = x.Sum(s => s.Saldo_ML),
                                                    DEBE_MS = x.Sum(s => s.DEBE_MS),
                                                    HABER_MS = x.Sum(s => s.HABER_MS),
                                                    Saldo_MS = x.Sum(s => s.Saldo_MS),
                                                    Cuenta_Padre = ct.CuentaPadre,
                                                    Bodega = string.Empty,
                                                    Editar = 0,
                                                    Linea = i
                                                }).ToList();




                            xrpAuxiliarConsolidado rpt2 = new xrpAuxiliarConsolidado();
                            rpt2.Parameters["P_Fecha1"].Value = Fecha1;
                            rpt2.Parameters["P_Fecha2"].Value = Fecha2;
                            rpt2.Parameters["P_Cuenta"].Value = Cuenta;
                            rpt2.Parameters["P_Bodega"].Value = (bo == null ? string.Empty : string.Concat(bo.Codigo, " - ", bo.Bodega));
                            rpt2.DataSource = qConsolidado;


                            stream = new MemoryStream();

                            if (Tipo == "PDF")
                            {
                                rpt2.ExportToPdf(stream, null);
                                stream.Seek(0, SeekOrigin.Begin);
                            }
                            else
                            {
                                rpt2.ExportToXlsx(stream, null);
                                stream.Seek(0, SeekOrigin.Begin);
                            }


                            datos = new();
                            datos.d = stream.ToArray();
                            datos.Nombre = "xrpAuxiliarConsolidado";
                            lstDatos.Add(datos);

                        }
                    }

       

                    if(Tipo == "EXCEL")
                    {
                        xrpAuxiliarExcel rptExcel = new xrpAuxiliarExcel();
                        rptExcel.Parameters["P_Fecha1"].Value = Fecha1;
                        rptExcel.Parameters["P_Fecha2"].Value = Fecha2;
                        rptExcel.Parameters["P_Cuenta"].Value = Cuenta;
                        rptExcel.Parameters["P_Bodega"].Value = (bo == null ? string.Empty : string.Concat(bo.Codigo, " - ", bo.Bodega));

                        rptExcel.DataSource = sqlDataSource;


                        stream = new MemoryStream();

                        rptExcel.ExportToXlsx(stream, null);
                        stream.Seek(0, SeekOrigin.Begin);




                        datos = new();
                        datos.d = stream.ToArray();
                        datos.Nombre = "xrpAuxiliarExcel";
                        lstDatos.Add(datos);

                    }




                    datos = new();
                    datos.d = Tipo;
                    datos.Nombre = "TIPO";
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
        public string GetAsiento(int IdAsiento, string NoDoc)
        {
            return V_GetAsiento(IdAsiento, NoDoc);
        }

        private string V_GetAsiento(int IdAsiento, string NoDoc)
        {
       

            string json = string.Empty;
            if (NoDoc == null) NoDoc = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new();

                    var qAsiento = (from _q in Conexion.AsientosContables
                                    where _q.IdAsiento == IdAsiento 
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
                                        _q.Automatico,
                                        _q.Revisado,
                                        AsientosContablesDetalle = _q.AsientosContablesDetalle.Where(w => w.DebitoML + w.CreditoML != 0 && w.NoDocumento == (NoDoc == string.Empty ? w.NoDocumento : NoDoc)).OrderBy( o => o.NoLinea).ToList(),

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




        [Route("api/Contabilidad/AuxiliarContable/GetOtrosDatos")]
        [HttpGet]
        public string GetOtrosDatos()
        {
            return V_GetOtrosDatos();
        }

        private string V_GetOtrosDatos()
        {
       
            string json = string.Empty;
            try
            {
                using (Conexion)
                {


                    List<Cls_Datos> lstDatos = new();


                    var qBogas = (from _q in Conexion.Bodegas
                                  select new
                                  {
                                      _q.IdBodega,
                                      _q.Codigo,
                                      _q.Bodega
                                  }).ToList();

                    Cls_Datos datos = new();
                    datos.Nombre = "BODEGAS";
                    datos.d = qBogas;

                    lstDatos.Add(datos);



                    var qCuentas = (from _q in Conexion.CatalogoCuenta
                                    select new
                                    {
                                        _q.CuentaContable,
                                        _q.NombreCuenta,
                                        _q.Nivel,
                                        _q.IdGrupo,
                                        Grupo = _q.GruposCuentas!.Nombre,
                                        _q.ClaseCuenta,
                                        _q.CuentaPadre,
                                        _q.Naturaleza,
                                        _q.Bloqueada,
                                        Filtro = string.Concat(_q.CuentaContable, " ", _q.NombreCuenta)
                                    }).ToList();

                    
                    datos = new();
                    datos.Nombre = "CUENTAS";
                    datos.d = qCuentas;
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
