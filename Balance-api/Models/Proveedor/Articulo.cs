using Balance_api.Models.Contabilidad;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Proveedor
{
    [Table("Articulos", Schema = "CXP")]
    public class Articulo
    {
        [Key]
        public int IdArticulo { get; set; }        
        public string Codigo { get; set; }        
        public string Descripcion { get; set; }        
        public string Marca { get; set; }        
        public string Modelo { get; set; }        
        public string Activo { get; set; }        
        public string FechaCreacion { get; set; }
        public string IdUsuarioCreacion { get; set; }

    }
}
