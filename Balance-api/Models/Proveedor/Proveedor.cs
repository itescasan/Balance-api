using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Balance_api.Models.Proveedor
{
    [Table("Proveedor", Schema = "CXP")]
    public class Proveedor
    {
        [Key]
        public int IdProveedor { get; set; }
        public string Codigo { get; set; }
        [Column("Proveedor")]
        public string Proveedor1 { get; set; }
        public string? NombreComercial { get; set; }
        public string? CUENTAXPAGAR { get; set; }

        public string? CUENTAANTICIPO { get; set; }
    }
}
