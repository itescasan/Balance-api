using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Inventario
{
    [Table("Bodegas", Schema = "INVFAC")]
    public class Bodegas
    {
        [Key]
        public int IdBodega { get; set; }
        public string Codigo { get; set; }
        public string Bodega { get; set; }
    }
}
