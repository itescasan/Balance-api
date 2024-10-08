
using Balance_api.Class;
using Balance_api.Contexts;
using Balance_api.Models.Contabilidad;
using Balance_api.Models.Proveedor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace Balance_api.Controllers.Proveedor
{
    public class GastosInternosController : Controller
    {

        private readonly BalanceEntities Conexion;

        public GastosInternosController(BalanceEntities db)
        {
            Conexion = db;
        }


        [Route("api/Proveedor/GastosInternos/Get")]
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

                    var qQuery = (from _q in Conexion.CatalogoGastosInternos
                                  join _p in Conexion.Proveedor on _q.COD_PROV equals _p.Codigo
                                  select new
                                  {
                                      _q.CODIGO,
                                      _q.DESCRIPCION,
                                      _q.CUENTACONTABLE,
                                      _q.APLICAREN,
                                      _q.TIPO,
                                      _q.ESTADO,
                                      _q.COD_PROV,
                                      PROVEEDOR = string.Concat(_q.COD_PROV, " ", _p.Proveedor1)
                                  }).ToList();



                    Cls_Datos datos = new();
                    datos.Nombre = "REGISTROS";
                    datos.d = qQuery;

                    json = Cls_Mensaje.Tojson(datos, qQuery.Count, string.Empty, string.Empty, 0);
                }



            }
            catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;
        }




        [Route("api/Proveedor/GastosInternos/GetDatos")]
        [HttpGet]
        public string GetDatos()
        {
            return V_GetDatos();
        }

        private string V_GetDatos()
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new();

                    var qCuentas = (from _q in Conexion.CatalogoCuenta
                                    where _q.ClaseCuenta == "D"
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

                    Cls_Datos datos = new();
                    datos.Nombre = "CUENTAS";
                    datos.d = qCuentas;
                    lstDatos.Add(datos);



                    var qProv = (from _q in Conexion.Proveedor
                                    select new
                                    {
                                        _q.Codigo,
                                        _q.Proveedor1
                                    }).ToList();

                    datos = new();
                    datos.Nombre = "PROVEEDOR";
                    datos.d = qProv;
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





        [Route("api/Proveedor/GastosInternos/Guardar")]
        [HttpPost]
        public IActionResult Guardar([FromBody] CatalogoGastosInternos d)
        {
            if (ModelState.IsValid)
            {

                return Ok(V_Guardar(d));

            }
            else
            {
                return BadRequest();
            }

        }

        private string V_Guardar(CatalogoGastosInternos d)
        {

            string json = string.Empty;

            try
            {

                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                using (Conexion)
                {

                    bool esNuevo = false;
                    
                    CatalogoGastosInternos? _Maestro = Conexion.CatalogoGastosInternos.Find(d.CODIGO);

                  

                    if (_Maestro == null)
                    {
                        int Codigo = 1;
                        _Maestro = new CatalogoGastosInternos();
                        Codigo = Conexion.CatalogoGastosInternos.Max(m => m.CODIGO);
                        _Maestro.CODIGO = Codigo + 1;
                        esNuevo = true;
                    }

                    _Maestro.COD_PROV = d.COD_PROV;
                    _Maestro.DESCRIPCION = d.DESCRIPCION;
                    _Maestro.CUENTACONTABLE = d.CUENTACONTABLE;
                    _Maestro.APLICAREN = d.APLICAREN;
                    _Maestro.TIPO = d.TIPO;
                    _Maestro.ESTADO = d.ESTADO;

                    if (esNuevo) Conexion.CatalogoGastosInternos.Add(_Maestro);

                    Conexion.SaveChanges();


                    if (esNuevo)
                    {
                        
                        Conexion.SaveChanges();
                    }
                        





                    Cls_Datos datos = new();
                    datos.Nombre = "GUARDAR";
                    datos.d = "Registro Guardado";



                    scope.Complete();

                    json = Cls_Mensaje.Tojson(datos, 1, string.Empty, string.Empty, 0);



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
