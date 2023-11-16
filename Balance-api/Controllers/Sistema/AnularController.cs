using Balance_api.Class;
using Balance_api.Class.Contabilidad;
using Balance_api.Contexts;
using Balance_api.Controllers.Contabilidad;
using Balance_api.Models.Contabilidad;
using Balance_api.Models.Sistema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace Balance_api.Controllers.Sistema
{
    public class AnularController : Controller
    {
        private readonly BalanceEntities Conexion;

        public AnularController(BalanceEntities db)
        {
            Conexion = db;
        }



        [Route("api/Documento/Anular")]
        [HttpPost]
        public IActionResult Anular(string IdDoc, string Motivo, string Tipo, string Usuario)
        {
            if (ModelState.IsValid)
            {

                return Ok(V_Anular(IdDoc, Motivo, Tipo, Usuario));

            }
            else
            {
                return BadRequest();
            }

        }

        public string V_Anular(string IdDoc, string Motivo, string Tipo, string Usuario)
        {


            string json = string.Empty;


            try
            {

                using TransactionScope scope = new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable });
                using (Conexion)
                {

                  

                    Asiento? A = null;
        
                    switch (Tipo)
                    {
                        case "Asiento":


                            A = Conexion.AsientosContables.Find(int.Parse(IdDoc))!;

                            A.Estado = "Anulado";
                            A.UsuarioUpdate = Usuario;
                            A.FechaUpdate = DateTime.Now;

                            break;

                        case "Transferencia":

                   
                            Transferencia Transf = Conexion.Transferencia.Find(Guid.Parse(IdDoc))!;

                            Transf.Anulado = true;
                            Transf.UsuarioAnula = Usuario;
                            Transf.FechaAnulacion = DateTime.Now;

                            MovimientoDoc[] mod = Conexion.MovimientoDoc.Where(w => w.NoDocOrigen == Transf.NoTransferencia && w.SerieOrigen == Transf.IdSerie && w.TipoDocumentoOrigen == "TRANSF").ToArray();

                            foreach(MovimientoDoc m in mod)
                            {
                                m.Activo = false;
                            }


                            A = Conexion.AsientosContables.FirstOrDefault(w => w.NoDocOrigen == Transf.NoTransferencia && w.IdSerieDocOrigen == Transf.IdSerie && w.TipoDocOrigen == (Transf.TipoTransferencia == "C" ? "TRANSFERENCIA A CUENTA" : "TRANSFERENCIA A DOCUMENTO"));

                            if(A != null)
                            {
                                A.Estado = "Anulado";
                                A.UsuarioUpdate = Transf.UsuarioAnula;
                                A.FechaUpdate = Transf.FechaAnulacion;
                            }
                            break;
                        case "Cheque":
                            Cheques cheque = Conexion.Cheque.Find(Guid.Parse(IdDoc))!;

                            cheque.Anulado = true;
                            cheque.UsuarioAnula = Usuario;
                            cheque.FechaAnulacion = DateTime.Now;

                            MovimientoDoc[] mod2 = Conexion.MovimientoDoc.Where(w => w.NoDocOrigen == cheque.NoCheque && w.SerieOrigen == cheque.IdSerie && w.TipoDocumentoOrigen == "Cheque").ToArray();

                            foreach (MovimientoDoc m in mod2)
                            {
                                m.Activo = false;
                            }


                            A = Conexion.AsientosContables.FirstOrDefault(w => w.NoDocOrigen == cheque.NoCheque && w.IdSerieDocOrigen == cheque.IdSerie && w.TipoDocOrigen ==  (cheque.TipoCheque == "C" ? "CHEQUE A CUENTA" : "CHEQUE A DOCUMENTO"));

                            if (A != null)
                            {
                                A.Estado = "Anulado";
                                A.UsuarioUpdate = cheque.UsuarioAnula;
                                A.FechaUpdate = cheque.FechaAnulacion;
                            }

                            break;
                    }



                    Cls_Datos datos = new();
                    datos.Nombre = "ANULAR";
                    datos.d = "Registro Anulado";

                    json = Cls_Mensaje.Tojson(datos, 1, string.Empty, string.Empty, 0);

                    Conexion.SaveChanges();
                    scope.Complete();



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
