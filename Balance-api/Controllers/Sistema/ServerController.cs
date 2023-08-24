using Balance_api.Class;
using Balance_api.Contexts;
using Microsoft.AspNetCore.Mvc;

namespace Balance_api.Controllers.Sistema
{
    //[Produces("application/json")]
    public class ServerController : Controller
    {

        private readonly BalanceEntities Conexion;

        public ServerController(BalanceEntities db)
        {
            Conexion = db;
        }
        [Route("api/Sistema/FechaServidor")]
        [HttpGet]
        public string FechaServidor()
        {
            return v_FechaServidor();
        }

        private string v_FechaServidor()
        {
            string json = string.Empty;
            try
            {
                using (Conexion)
                {
                    List<Cls_Datos> lstDatos = new List<Cls_Datos>();

                    Cls_Datos datos = new Cls_Datos();
                    datos.Nombre = "FECHA SERVIDOR";
                    datos.d = DateTime.Now;
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
