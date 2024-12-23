using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Balance_api.Models.Proveedor
{
    [Table("OrdenCompraCentrogasto", Schema = "CXP")]
    public class OrdenCompraCentrogasto
    {
        [Key]
        public int IdCentroGasto { get; set; }
        public int IdOrdenCompra { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string Bodega { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal Participacion1 { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string Rubro { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string CuentaContable { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string CentroCosto { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal Participacion2 { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int IdUsuarioCreacion { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string UsuarioCreacion { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string NoDocOrigen { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string TipoDocOrigen { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string ProvieneDe { get; set; }


        [ForeignKey("IdOrdenCompra")]
        public OrdenCompra OrdenCompra { get; set; }
    }
}
