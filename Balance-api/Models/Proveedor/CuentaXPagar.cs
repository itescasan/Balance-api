using Balance_api.Models.Contabilidad;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Balance_api.Models.Proveedor
{
    [Table("CuentaXPagar", Schema = "CXP")]
    public class CuentaXPagar
    {
        [Key]
        public int IdCuentasXPagar { get; set; }

        public int IdOrdenCompra { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string NoSolicitud { get; set; }



    }
}
