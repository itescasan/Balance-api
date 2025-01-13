using Balance_api.Models.Contabilidad;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Balance_api.Models.Proveedor
{
    [Table("OrdenCompra", Schema = "CXP")]
    public class OrdenCompra
    {
        [Key]
        public int IdOrdenCompra { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string NoOrdenCompra { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string NoSolicitud { get; set; }


        [Column(TypeName = "nvarchar(50)")]
        public string TipoDocOrigen { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string CodigoProveedor { get; set; }


        [Column(TypeName = "nvarchar(50)")]
        public string CuentaContableSolicitante { get; set; }

        [Column(TypeName = "nvarchar(10)")]
        public string CodigoBodega { get; set; }


        
        [Column(TypeName = "nvarchar(50)")]
        public string Estado { get; set; }

   

        
        [ForeignKey("IdOrdenCompra")]
        public ICollection<OrdenCompraCentrogasto> OrdenCompraCentrogasto { get; set; }

    }
}
