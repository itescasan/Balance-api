using Balance_api.Class;
using Balance_api.Contexts;
using Balance_api.Controllers.Sistema;
using Balance_api.Models.Contabilidad;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace Balance_api.Controllers.Contabilidad
{
    public class AsientoController : Controller
    {
        private readonly BalanceEntities Conexion;

        public AsientoController(BalanceEntities db)
        {
            Conexion = db;
        }


        [Route("api/Contabilidad/AsientoContable/Datos")]
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
                                    where _q.ClaseCuenta == "D"
                                    select new
                                    {
                                        _q.CuentaContable,
                                        NombreCuenta =  string.Concat(_q.CuentaContable, " ", _q.NombreCuenta),
                                        _q.ClaseCuenta,
                                        _q.Naturaleza,
                                        _q.Bloqueada
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




        [Route("api/Contabilidad/Asiento/Guardar")]
        [HttpPost]
        public IActionResult Guardar([FromBody] CatalogoCuenta d)
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

        private string V_Guardar(CatalogoCuenta d)
        {

            string json = string.Empty;

            try
            {

                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                using (Conexion)
                {

                    bool esNuevo = false;
                    CatalogoCuenta? _Maestro = Conexion.CatalogoCuenta.Find(d.CuentaContable);


                    if (_Maestro == null)
                    {
                        _Maestro = new CatalogoCuenta();
                        _Maestro.FechaCreacion = DateTime.Now;
                        _Maestro.UsuarioReg = d.UsuarioModifica;
                        esNuevo = true;
                    }

                    if (d.ClaseCuenta == "D")
                    {
                        CatalogoCuenta? _Padre = Conexion.CatalogoCuenta.Find(d.CuentaPadre);
                        CatalogoCuenta? _Hija = Conexion.CatalogoCuenta.FirstOrDefault(f => f.CuentaPadre == d.CuentaContable && f.ClaseCuenta == "G");

                        if (_Padre == null)
                        {
                            json = Cls_Mensaje.Tojson(null, 0, "1", "No se ha definido una cuenta padre.", 1);
                            return json;
                        }

                        if (_Padre.ClaseCuenta == "D")
                        {
                            json = Cls_Mensaje.Tojson(null, 0, "1", "La cuenta padre esta definida como Detalle.", 1);
                            return json;
                        }


                        if (_Hija != null)
                        {
                            json = Cls_Mensaje.Tojson(null, 0, "1", "No se puede cambiar la clase de la cuenta, por favor modifique las cuentas hijas.", 1);
                            return json;

                        }

                    }



                    _Maestro.CuentaContable = d.CuentaContable;
                    _Maestro.NombreCuenta = d.NombreCuenta;
                    _Maestro.Nivel = d.Nivel;
                    _Maestro.IdGrupo = d.IdGrupo;
                    _Maestro.ClaseCuenta = d.ClaseCuenta;
                    _Maestro.CuentaPadre = d.CuentaPadre;
                    _Maestro.Naturaleza = d.Naturaleza;
                    _Maestro.Bloqueada = d.Bloqueada;
                    _Maestro.UsuarioModifica = d.UsuarioModifica;
                    _Maestro.FechaUpdate = DateTime.Now;
                    if (esNuevo) Conexion.CatalogoCuenta.Add(_Maestro);

                    Conexion.SaveChanges();




                    List<Cls_Datos> lstDatos = new();


                    Cls_Datos datos = new();
                    datos.Nombre = "GUARDAR";
                    datos.d = "Registro Guardado";
                    lstDatos.Add(datos);




                    scope.Complete();

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
