using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Contabilidad
{
    [Table("GruposCuentas", Schema = "CNT")]
    public class GruposCuentas
    {
        [Key]
        public int IdGrupo { get; set; }
        public required string Nombre { get; set; }
    }
}
