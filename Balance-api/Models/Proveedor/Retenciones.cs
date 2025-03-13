using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Proveedor
{
    [Table("Retenciones", Schema = "CXP")]
    public class Retenciones
    {

        [Key]
        public int IdRetencion { get; set; }
        public string TipoImpuesto { get; set; }
        public string Retencion { get; set; }
        public decimal? Porcentaje { get; set; }
        public string? CuentaContable { get; set; }
        public bool? AplicaEnCXC { get; set; }
        public bool? AplicaEnCXP { get; set; }
        public string? Naturaleza { get; set; }
        public bool? AplicarAutomatico { get; set; }
    }
}
