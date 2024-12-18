using Balance_api.Class;
using Balance_api.Contexts;
using Balance_api.Models.Contabilidad;
using Balance_api.Models.Proveedor;
using Balance_api.Models.Sistema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace Balance_api.Controllers.Proveedor
{
    public class ArticuloController : Controller
    {
        private readonly BalanceEntities Conexion;

        public ArticuloController(BalanceEntities db)
        {
            Conexion = db;
        }



        [Route("api/Proveedor/Articulo/Get")]
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

                    var qQuery = (from _q in Conexion.Articulos
                                  select new
                                  {
                                      _q.Codigo,
                                      _q.Descripcion,
                                      _q.Marca,
                                      _q.Modelo,
                                      Activo = (_q.Activo == "True") ? "Activo" : "Inactivo"
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



        [Route("api/Proveedor/Articulo/Guardar")]
        [HttpPost]
        public IActionResult Guardar([FromBody] Articulo d)
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

        private string V_Guardar(Articulo d)
        {

            string json = string.Empty;

            try
            {

                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                using (Conexion)
                {

                    bool esNuevo = false;

                    Articulo? _Maestro = Conexion.Articulos.Find(d.Descripcion);

                    if (_Maestro == null)
                    {
                        _Maestro = new Articulo();
                        esNuevo = true;
                    }

                    _Maestro.Codigo = d.Codigo;
                    _Maestro.Descripcion = d.Descripcion;
                    _Maestro.Marca = d.Marca;
                    _Maestro.Modelo = d.Modelo;
                    _Maestro.IdUsuarioCreacion = d.IdUsuarioCreacion;

                    if (esNuevo) Conexion.Articulos.Add(_Maestro);

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
