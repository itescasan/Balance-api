using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Proveedor
{
    [Table("CatalogoGastosInternos", Schema = "CXP")]
    public class CatalogoGastosInternos
    {
        [Key]
        public int CODIGO { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string DESCRIPCION { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string CUENTACONTABLE { get; set; }
        [Column(TypeName = "nvarchar(1)")]
        public string APLICAREN { get; set; }
        [Column(TypeName = "nvarchar(1)")]
        public string TIPO { get; set; }
        [Column(TypeName = "nvarchar(1)")]
        public string ESTADO { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string COD_PROV { get; set; }



        [ForeignKey("IdOrdenCompra")]
        public ICollection<OrdenCompra>  OrdenCompra { get; set; }


    }
}
