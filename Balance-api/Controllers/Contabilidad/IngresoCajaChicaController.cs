using Azure;
using Balance_api.Class;
using Balance_api.Class.Contabilidad;
using Balance_api.Contexts;
using Balance_api.Models.Contabilidad;
using Balance_api.Models.Sistema;
using DevExpress.CodeParser;
using DevExpress.PivotGrid.OLAP;
using DevExpress.Xpo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Transactions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Balance_api.Controllers.Contabilidad
{
    public class IngresoCajaChica : Controller
    {
        private readonly BalanceEntities Conexion;

        public IngresoCajaChica(BalanceEntities db) 
        {
            Conexion = db;
        }




        [Route("api/Contabilidad/IngresoCajaChica/Get")]
        [HttpGet]
        public string Get(int Consecutivo, string Usuario, string CuentaBodega )
        {
            return V_Get(Consecutivo, Usuario, CuentaBodega);
        }

        private string V_Get(int Consecutivo, string Usuario, string CuentaBodega)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new();

                    var datos = V_Obterner_IngresoCaja(Consecutivo, Usuario, CuentaBodega);
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

        private Cls_Datos V_Obterner_IngresoCaja(int Consecutivo, string Usuario, string CuentaBodega)
        {

            var qIngresoCaja = (from _q in Conexion.IngresoC
                                join _d in Conexion.DetIngCaja on _q.IdIngresoCajaChica equals _d.IdIngresoCajaC
                                join _c in Conexion.CatalogoCentroCostos on _d.CentroCosto equals _c.Codigo
                                join _e in Conexion.CatalogoCuenta on _d.Cuenta equals _e.CuentaContable
                                join _ca in Conexion.CatalogoCuenta on _d.CuentaEmpleado equals _ca.CuentaContable into union_ca_d
                                from _ca_d in union_ca_d.DefaultIfEmpty()
                                where _q.Usuario == Usuario && _q.Aplicado == false && _q.Contabilizado == false && _q.Cuenta == CuentaBodega && _q.Consecutivo == Consecutivo
                                orderby _d.IdDetalleIngresoCajaChica descending
                                select new
                                {
                                    _d.IdDetalleIngresoCajaChica,
                                    _d.IdIngresoCajaC,
                                    _d.FechaRegistro,
                                    _d.FechaFactura,
                                    _d.Concepto,
                                    _d.Referencia,
                                    _d.Proveedor,
                                    Cuenta = string.Concat(_e.CuentaContable, " ", _e.NombreCuenta),
                                    _c.CentroCosto,
                                    _d.SubTotal,
                                    _d.Iva,
                                    _d.Total,
                                     CuentaEmpleado = _ca_d.NombreCuenta,
                                    _d.FechaModificacion
                                }).ToList();


           
            Cls_Datos datos = new Cls_Datos();
            datos.Nombre = "INGRESOCAJA";
            datos.d = qIngresoCaja;


            return datos;
        }


        [Route("api/Contabilidad/IngresoCajaChica/Datos")]
        [HttpGet]
        public string IngresoCaja_Chica(string user)
        {
            return V_IngresoCajaChicaCC(user);
        }

        private string V_IngresoCajaChicaCC(string user)
        {
            string json = string.Empty;
            try
            {
                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                using (Conexion)
                {


                    Usuarios? Usuario = Conexion.Usuarios.FirstOrDefault(f => f.Usuario == user &&  f.AccesoWeb);

                    if (Usuario == null)
                    {
                        json = Cls_Mensaje.Tojson(null, 0, "1", "No tiene permiso para acceder a la inforacion solicitada.", 1);
                        return json;
                    }



                    List<Cls_Datos> lstDatos = new List<Cls_Datos>();
                    Cls_Datos datos = new Cls_Datos();


                    var qCuentasC = (from _q in Conexion.AccesoCajaChica
                                     where _q.Usuario == user && _q.Activo == true
                                     select new
                                     {
                                         _q.CuentaContable,
                                         NombreCuenta = string.Concat(_q.CuentaContable, " ", _q.NombreCuenta)
                                     }).ToList();

                    datos = new Cls_Datos();
                    datos.Nombre = "ACCESOCAJA CHICA";
                    datos.d = qCuentasC;
                    lstDatos.Add(datos);                    

                    var qCentroC = (from _q in Conexion.CatalogoCentroCostos
                                    select new
                                    {
                                        _q.IdCatalogoCentroCosto,
                                        _q.Codigo,
                                        _q.CentroCosto
                                    }).ToList();

                    datos = new Cls_Datos();
                    datos.Nombre = "CENTROCOSTO";
                    datos.d = qCentroC;
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

        [Route("api/Contabilidad/IngresoCajaChica/Rubro")]
        [HttpGet]
        public string IngresoCaja_Rubro(string CuentaPadre)
        {
            return V_IngresoCajaChicaRubro(CuentaPadre);
        }

        private string V_IngresoCajaChicaRubro(string CuentaPadre)
        {
            string json = string.Empty;
            try
            {
                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                using (Conexion)
                {
                    var cuentasAsociadas = Conexion.CuentasAsociadas
                                           .Where(f => f.CuentaPadre == CuentaPadre)
                                           .Select(f => f.CuentaAsociada)   // <-- aquí debe ser la propiedad correcta
                                           .ToList();

                    List<Cls_Datos> lstDatos = new List<Cls_Datos>();
                    Cls_Datos datos = new Cls_Datos();

                    if (!cuentasAsociadas.Any())
                    {
                        cuentasAsociadas.Add(CuentaPadre);
                    }


                    var qRubros = (from _q in Conexion.CatalogoCuenta
                                   join _c in Conexion.CatalogoCuenta 
                                   on _q.CuentaPadre equals _c.CuentaContable
                                   where _q.Nivel == 6   && (_q.IdGrupo == 5 || _q.IdGrupo == 0 ) && cuentasAsociadas.Contains(_q.CuentaPadre!)
                                   select new
                                   {
                                       _q.CuentaContable,
                                       NombreCuenta = string.Concat(_q.CuentaContable, "->", _q.NombreCuenta, "->", _c.NombreCuenta),
                                   }).ToList();

                    datos = new Cls_Datos();
                    datos.Nombre = "RUBRO CAJA CHICA";
                    datos.d = qRubros;
                    lstDatos.Add(datos);

                    var qConsecutivo = (from _q in Conexion.ConfCaja
                                        where _q.CuentaContable == CuentaPadre
                                   select new
                                   {
                                       _q.CuentaContable,_q.Consecutivo,_q.Serie,_q.Valor
                                   }).ToList();

                    datos = new Cls_Datos();
                    datos.Nombre = "INFOCAJA";
                    datos.d = qConsecutivo;
                    lstDatos.Add(datos);

                    var qCuentaEmpleado = (from _q in Conexion.CatalogoCuenta
                                           where _q.Nivel == 6 && _q.IdGrupo == 0 && _q.CuentaPadre == "1103-03-01" && _q.Naturaleza == "D"
                                           select new
                                           {
                                               _q.CuentaContable,
                                               NombreCuenta = string.Concat(_q.CuentaContable, " ", _q.NombreCuenta),
                                           }).ToList();

                    datos = new Cls_Datos();
                    datos.Nombre = "CUENTAEMPLEADO";
                    datos.d = qCuentaEmpleado;
                    lstDatos.Add(datos);

                    //var qEnviadoCorregido = (from _q in Conexion.IngresoC
                    //                       where _q.Cuenta == CuentaPadre
                    //                       select _q).Count();

                    //datos = new Cls_Datos();
                    //datos.Nombre = "VALIDACION";
                    //datos.d = qEnviadoCorregido;
                    //lstDatos.Add(datos);

                    //var qContanbilizado = (from _q in Conexion.IngresoC
                    //                       where _q.Cuenta == CuentaPadre && _q.Aplicado == true && _q.Contabilizado == false
                    //                       select _q).Count();

                    //datos = new Cls_Datos();
                    //datos.Nombre = "CONTADOR";
                    //datos.d = qContanbilizado;
                    //lstDatos.Add(datos);



                    json = Cls_Mensaje.Tojson(lstDatos, lstDatos.Count, string.Empty, string.Empty, 0);
                }



            }
            catch (Exception ex)
            {
                json = Cls_Mensaje.Tojson(null, 0, "1", ex.Message, 1);
            }

            return json;
        }


        [Route("api/Contabilidad/IngresoCajaChica/Detalle")]
        [HttpGet]
        public string GetD(int IdDetalleIngresoCajaChica)
        {
            return V_GetDetalle(IdDetalleIngresoCajaChica);
        }

        private string V_GetDetalle(int IdDetalleIngresoCajaChica)
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new();

                    var datos = V_Obterner_Detalle(IdDetalleIngresoCajaChica);
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



        private Cls_Datos V_Obterner_Detalle(int IdDetalleIngresoCajaChica)
        {

            var qDetalle = (from _q in Conexion.DetIngCaja
                             where _q.IdDetalleIngresoCajaChica == IdDetalleIngresoCajaChica
                             select new
                             {
                                 _q.IdDetalleIngresoCajaChica,
                                 _q.IdIngresoCajaC,
                                 _q.FechaRegistro,
                                 _q.Concepto,
                                 _q.Referencia,
                                 _q.Proveedor,
                                 _q.Cuenta,
                                 _q.CentroCosto,
                                 _q.SubTotal,
                                 _q.Iva,
                                 _q.Total,
                                 _q.CuentaEmpleado,                                
                                 _q.FechaModificacion
                             }).ToList();

            Cls_Datos datos = new Cls_Datos();
            datos.Nombre = "DETALLE_INGRESOCAJA";
            datos.d = qDetalle;

            return datos;
        }

        [Route("api/Contabilidad/IngresoCajaChica/Guardar")]
        [HttpPost]
        public IActionResult Guardar([FromBody] Cls_Datos_IngresoCaja d)
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

        private string V_Guardar(Cls_Datos_IngresoCaja d)
        {

            string json = string.Empty;

            try
            {

                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                using (Conexion)
                {

                    bool esNuevo = false;
                    bool EsNuevoDet = false;
                    IngresoCaja? _Maestro = Conexion.IngresoC.Find(d.I.IdIngresoCajaChica);




                    if (_Maestro == null)
                    {
                        _Maestro = new IngresoCaja();
                        _Maestro.FechaRegistro = DateTime.Now;
                        _Maestro.Usuario = d.I.Usuario;
                        esNuevo = true;                        
                    }



                    //_Maestro.IdIngresoCajaChica = d.I.IdIngresoCajaChica;
                    _Maestro.Cuenta = d.I.Cuenta;
                    _Maestro.Consecutivo = d.I.Consecutivo;                   
                    _Maestro.UsuarioModifica = d.I.UsuarioModifica;
                    _Maestro.Aplicado = d.I.Aplicado;
                    _Maestro.Contabilizado = d.I.Contabilizado;
                    _Maestro.Corregir = d.I.Corregir;
                    

                    _Maestro.FechaModificacion = DateTime.Now;
                    if (esNuevo) Conexion.IngresoC.Add(_Maestro);

                    Conexion.SaveChanges();
                    
                    DetalleIngresoCaja? _Detalle = Conexion.DetIngCaja.Find(d.D.IdDetalleIngresoCajaChica);

                    EsNuevoDet = false;


                    if (_Detalle == null)
                    {
                        _Detalle = new DetalleIngresoCaja();
                        //_Detalle.IdDetalleIngresoCajaChica = d.D.IdDetalleIngresoCajaChica;
                        _Detalle.FechaRegistro = DateTime.Now;
                        _Detalle.FechaFactura = d.D.FechaFactura;
                        _Detalle.IdIngresoCajaC = _Maestro.IdIngresoCajaChica;
                        EsNuevoDet = true;
                    }


                    _Detalle.Concepto = d.D.Concepto;
                    _Detalle.Referencia = d.D.Referencia;
                    _Detalle.Proveedor = d.D.Proveedor;
                    _Detalle.Cuenta = d.D.Cuenta;
                    _Detalle.CentroCosto = d.D.CentroCosto;
                    _Detalle.SubTotal = d.D.SubTotal;
                    _Detalle.Iva = d.D.Iva;
                    _Detalle.Total = d.D.Total;
                    _Detalle.CuentaEmpleado = d.D.CuentaEmpleado;                        
                    _Detalle.Total = d.D.Total;
                    _Detalle.FechaModificacion = d.D.FechaModificacion;

                    if (EsNuevoDet) Conexion.DetIngCaja.Add(_Detalle);
                    Conexion.SaveChanges();

                    



                    List<Cls_Datos> lstDatos = new();


                    Cls_Datos datos = new Cls_Datos();
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


        [Route("api/Contabilidad/IngresoCajaChica/Eliminar")]
        [HttpPost]
        public IActionResult Eliminar(int IdIngCajaDetalle)
        {
            if (ModelState.IsValid)
            {

                return Ok(_Eliminar(IdIngCajaDetalle));

            }
            else
            {
                return BadRequest();
            }

        }

        private string _Eliminar(int IdIngCajaDetalle)
        {
            string json = string.Empty;

            try
            {

                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                    using (Conexion)
                        {

                    DetalleIngresoCaja? det = Conexion.DetIngCaja.FirstOrDefault(f => f.IdDetalleIngresoCajaChica == IdIngCajaDetalle);

                    if (det != null)
                    {
                        Conexion.DetIngCaja.Remove(det!);
                        Conexion.SaveChanges();
                    }

      
                    List<Cls_Datos> lstDatos = new();


                    Cls_Datos datos = new Cls_Datos();
                    datos.Nombre = "ELIMINAR";
                    datos.d = "Registro Eliminado";
                    lstDatos.Add(datos);

                    //FIN 
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

        [Route("api/Contabilidad/IngresoCajaChica/Registro")]
        [HttpGet]

        public IActionResult Registro(string User)
        {
            if (ModelState.IsValid)
            {

                return Ok(_Registro(User));

            }
            else
            {
                return BadRequest();
            }

        }

        private string _Registro(string User)
        {
            string json = string.Empty;
            try
            {
                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                using (Conexion)
                {

                    List<Cls_Datos> lstDatos = new List<Cls_Datos>();
                    Cls_Datos datos = new Cls_Datos();


                    var qRegistroIngCaja = (from _q in Conexion.IngresoC
                                            join _d in Conexion.CatalogoCuenta on _q.Cuenta equals _d.CuentaContable
                                            where _q.Usuario == User
                                            orderby _q.Consecutivo descending
                                            select new
                                           {
                                               _q.IdIngresoCajaChica,
                                               _q.FechaRegistro,
                                                Cuenta = string.Concat(_d.CuentaContable, " ", _d.NombreCuenta),
                                               _q.Consecutivo,                                                                                              
                                               _q.Usuario,
                                               _q.Enviado,
                                               _q.Corregir,
                                               _q.Aplicado,
                                               _q.Contabilizado,                                               
                                           }).ToList();

                    datos = new Cls_Datos();
                    datos.Nombre = "REGISTRO";
                    datos.d = qRegistroIngCaja;
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



        [Route("api/Contabilidad/IngresoCajaChica/Registro2")]
        [HttpGet]

        public IActionResult Registro2()
        {
            if (ModelState.IsValid)
            {

                return Ok(_Registro2());

            }
            else
            {
                return BadRequest();
            }

        }

        private string _Registro2()
        {
            string json = string.Empty;
            try
            {
                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                using (Conexion)
                {

                    List<Cls_Datos> lstDatos = new List<Cls_Datos>();
                    Cls_Datos datos = new Cls_Datos();


                    var qRegistroIngCaja2 = (from _q in Conexion.IngresoC
                                            join _d in Conexion.CatalogoCuenta on _q.Cuenta equals _d.CuentaContable
                                            where _q.Enviado == true && _q.Aplicado == false && _q.Contabilizado == false
                                            orderby _q.Consecutivo descending
                                            select new
                                            {
                                                _q.IdIngresoCajaChica,
                                                _q.FechaRegistro,
                                                Cuenta = string.Concat(_d.CuentaContable, " ", _d.NombreCuenta),
                                                _q.Consecutivo,
                                                _q.Usuario,
                                                _q.Enviado,
                                                _q.Aplicado,                                               
                                                _q.Corregir,
                                                _q.Contabilizado,
                                                
                                            }).ToList();

                    datos = new Cls_Datos();
                    datos.Nombre = "REGISTRO2";
                    datos.d = qRegistroIngCaja2;
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


        [Route("api/Contabilidad/IngresoCajaChica/Enviar")]
        [HttpPost]
        public IActionResult Enviar(int IdIngresoCaja, string user)
        {
            if (ModelState.IsValid)
            {

                return Ok(_Enviar(IdIngresoCaja, user));

            }
            else
            {
                return BadRequest();
            }

        }

        private string _Enviar(int IdIngresoCaja, string user)
        {
            string json = string.Empty;

            try
            {

                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                using (Conexion)
                {

                    IngresoCaja? det = Conexion.IngresoC.FirstOrDefault(f => f.IdIngresoCajaChica == IdIngresoCaja);

                    if (det != null)
                    {
                        //Conexion.DetIngCaja.Remove(det!);
                        det.Enviado = true;
                        if (det.Corregir == "Pendiente")
                        {
                            det.Corregir = "Completado";
                        }
                        det.UsuarioModifica = user;
                        det.FechaModificacion = DateTime.Now;
                        Conexion.SaveChanges();
                        //Conexion.Database.ExecuteSqlRaw($"UPDATE CNT.ConfCajaChica SET Consecutivo += 1  WHERE  CuentaContable = '{det.Cuenta}'");
                    }


                    List<Cls_Datos> lstDatos = new();


                    Cls_Datos datos = new Cls_Datos();
                    datos.Nombre = "Enviar";
                    datos.d = "Registro Enviado a revisión";
                    lstDatos.Add(datos);

                    //FIN 
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


        [Route("api/Contabilidad/IngresoCajaChica/Corregir")]
        [HttpPost]
        public IActionResult Corregir(int IdIngresoCaja, string user)
        {
            if (ModelState.IsValid)
            {

                return Ok(_Corregir(IdIngresoCaja, user));

            }
            else
            {
                return BadRequest();
            }

        }

        private string _Corregir(int IdIngresoCaja, string user)
        {
            string json = string.Empty;

            try
            {

                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                using (Conexion)
                {

                    IngresoCaja? det = Conexion.IngresoC.FirstOrDefault(f => f.IdIngresoCajaChica == IdIngresoCaja);

                    if (det != null)
                    {
                        //Conexion.DetIngCaja.Remove(det!);
                        det.Corregir = "Pendiente";
                        det.UsuarioModifica = user;
                        det.FechaModificacion = DateTime.Now;
                        Conexion.SaveChanges();
                        //Conexion.Database.ExecuteSqlRaw($"UPDATE CNT.ConfCajaChica SET Consecutivo += 1  WHERE  CuentaContable = '{det.Cuenta}'");
                    }


                    List<Cls_Datos> lstDatos = new();


                    Cls_Datos datos = new Cls_Datos();
                    datos.Nombre = "Corregir";
                    datos.d = "Registro Enviado para su Correccion";
                    lstDatos.Add(datos);

                    //FIN 
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


        [Route("api/Contabilidad/IngresoCajaChica/Aplicar")]
        [HttpPost]
        public IActionResult Aplicar(int IdIngresoCaja, string user)
        {
            if (ModelState.IsValid)
            {

                return Ok(_Aplicar(IdIngresoCaja, user));

            }
            else
            {
                return BadRequest();
            }

        }

        private string _Aplicar(int IdIngresoCaja, string user)
        {
            string json = string.Empty;

            try
            {

                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                using (Conexion)
                {

                    IngresoCaja? det = Conexion.IngresoC.FirstOrDefault(f => f.IdIngresoCajaChica == IdIngresoCaja);

                    if (det != null)
                    {
                        //Conexion.DetIngCaja.Remove(det!);
                        det.Aplicado = true;
                        det.UsuarioModifica = user;
                        det.FechaModificacion = DateTime.Now;
                        Conexion.SaveChanges();
                        Conexion.Database.ExecuteSqlRaw(@"UPDATE C
                                                        SET C.Consecutivo = C.Consecutivo + 1
                                                        FROM CNT.ConfCajaChica C
                                                        INNER JOIN CNT.IngresosCajaChica IC 
                                                        ON C.CuentaContable = IC.Cuenta
                                                        WHERE IC.Cuenta = {0}", det.Cuenta);

                    }
                    //UPDATE C
                    //Set C.Consecutivo += 1
                    //FROM CNT.ConfCajaChica C inner join
                    //CNT.IngresosCajaChica IC ON C.CuentaContable = IC.Cuenta
                    //WHERE IC.Cuenta = '6101-02-01'

                    List<Cls_Datos> lstDatos = new();


                    Cls_Datos datos = new Cls_Datos();
                    datos.Nombre = "Aplicar";
                    datos.d = "Registro Aplicado Correctamente";
                    lstDatos.Add(datos);

                    //FIN 
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


        [Route("api/Contabilidad/IngresoCajaChica/ValidarCaja")]
        [HttpGet]
        public string GetDatosCaja(int Consecutivo, string CuentaPadre)
        {
            return V_GetDatosCaja(Consecutivo, CuentaPadre);
        }

        private string V_GetDatosCaja(int Consecutivo, string CuentaPadre)
        {
            string json = string.Empty;
            try
            {
                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                using (Conexion)
                {

                    List<Cls_Datos> lstDatos = new List<Cls_Datos>();
                    Cls_Datos datos = new Cls_Datos();



                    var qEnviadoCorregido = (from _q in Conexion.IngresoC
                                             where _q.Cuenta == CuentaPadre && _q.Consecutivo == Consecutivo
                                             select _q).ToList();

                    datos = new Cls_Datos();
                    datos.Nombre = "VALIDACION";
                    datos.d = qEnviadoCorregido;
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
