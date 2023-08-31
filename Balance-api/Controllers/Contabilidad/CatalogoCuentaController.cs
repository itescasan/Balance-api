using Balance_api.Class;
using Balance_api.Contexts;
using Balance_api.Models.Contabilidad;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace Balance_api.Controllers.Contabilidad
{
   // [Route("api/Contabilidad/[Controller]")]
    public class CatalogoCuentaController : Controller
    {

        private readonly BalanceEntities Conexion;

        public CatalogoCuentaController(BalanceEntities db)
        {
            Conexion = db;
        }


        [Route("api/Contabilidad/CatalogoCuenta/Get")]
        [HttpGet]
        public  string Get()
        {
            return  v_Get();
        }

        private string v_Get()
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new List<Cls_Datos>();

                    var datos = v_Obterner_Cuentas();
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

        private Cls_Datos v_Obterner_Cuentas()
        {

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

            Cls_Datos datos = new Cls_Datos();
            datos.Nombre = "CUENTAS";
            datos.d = qCuentas;

            return datos;
        }


   


        [Route("api/Contabilidad/CatalogoCuenta/Datos")]
        [HttpGet]
        public string Datos()
        {
            return  v_Datos();
        }

        private string v_Datos()
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new List<Cls_Datos>();

                    Cls_Datos datos = v_Obterner_Cuentas();

                    lstDatos.Add(datos);


                    var qGrupos = Conexion.GruposCuentas.ToList();

                    datos = new Cls_Datos();
                    datos.Nombre = "GRUPOS";
                    datos.d = qGrupos;
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




        [Route("api/Contabilidad/CatalogoCuenta/Guardar")]
        [HttpPost]
        public IActionResult Guardar([FromBody] CatalogoCuenta d)
        {
            if (ModelState.IsValid)
            {

                return Ok(v_Guardar(d));

            }
            else
            {
                return BadRequest();
            }

        }

        private string v_Guardar(CatalogoCuenta d)
        {

            string json = string.Empty;

            try
            {

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
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
                        if (esNuevo)  Conexion.CatalogoCuenta.Add(_Maestro);

                         Conexion.SaveChanges();




                        List<Cls_Datos> lstDatos = new List<Cls_Datos>();


                        Cls_Datos datos = new Cls_Datos();
                        datos.Nombre = "GUARDAR";
                        datos.d = "Registro Guardado";
                        lstDatos.Add(datos);


                       



                        json = Cls_Mensaje.Tojson(lstDatos, lstDatos.Count, string.Empty, string.Empty, 0);

                        scope.Complete();

                    }
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
