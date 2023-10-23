using Balance_api.Class;
using Balance_api.Class.Contabilidad;
using Balance_api.Contexts;
using Balance_api.Controllers.Contabilidad;
using Balance_api.Models.Contabilidad;
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


                            A = Conexion.AsientosContables.Find(int.Parse(IdDoc));

                            A.Estado = "Anulado";
                            A.UsuarioUpdate = Usuario;
                            A.FechaUpdate = DateTime.Now;

                            break;

                        case "Transferencia":

                   
                            Transferencia Transf = Conexion.Transferencia.Find(Guid.Parse(IdDoc))!;

                            Transf.Anulado = true;
                            Transf.UsuarioAnula = Usuario;
                            Transf.FechaAnulacion = DateTime.Now;

                            A = Conexion.AsientosContables.FirstOrDefault(w => w.NoDocOrigen == Transf.NoTransferencia && w.IdSerieDocOrigen == Transf.IdSerie && w.TipoDocOrigen == (Transf.TipoTransferencia == "C" ? "TRANSFERENCIA A CUENTA" : "TRANSFERENCIA A DOCUMENTO"));

                            if(A != null)
                            {
                                A.Estado = "Anulado";
                                A.UsuarioUpdate = Transf.UsuarioAnula;
                                A.FechaUpdate = Transf.FechaAnulacion;
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
