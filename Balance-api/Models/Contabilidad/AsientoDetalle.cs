using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Contabilidad
{
    [Table("AsientosContablesDetalle", Schema = "CNT")]
    public class AsientoDetalle
    {
        [Key]
        public int IdDetalleAsiento { get; set; }
        public int IdAsiento { get; set; }
        public int NoLinea { get; set; }
        public string CuentaContable { get; set; }
        public decimal Debito { get; set; }
        public decimal DebitoML { get; set; }
        public decimal DebitoMS { get; set; }
        public decimal Credito { get; set; }
        public decimal CreditoML { get; set; }
        public decimal CreditoMS { get; set; }
        public string Modulo { get; set; }
        public string Descripcion { get; set; }
        public string Referencia { get; set; }
        public string Naturaleza { get; set; }

    }
}
