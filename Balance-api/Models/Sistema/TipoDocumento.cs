using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Sistema
{
    [Table("TipoDocumento", Schema = "CNT")]
    public class TipoDocumento
    {
        [Key]
        public int IdTipoDocumento { get; set; }
        public string Abreviatura { get; set; }
        public string Descripcion { get; set; }
        public bool Automatico { get; set; }
    }
}
