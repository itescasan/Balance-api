using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Balance_api.Models.Sistema
{
    [Table("AccesoWeb", Schema = "SIS")]
    public class AccesoWeb
    {
        [Key]
        public int IdAcceso { get; set; }
        public bool EsMenu { get; set; }
        public string Id { get; set; }
        public string Caption { get; set; }
        public string MenuPadre { get; set; }
        public string Clase { get; set; }
        public string Usuario { get; set; }
        public string Modulo { get; set; }     
        public bool Activo { get; set; }
    }
}
