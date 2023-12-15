using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Contabilidad
{
    [Table("ChequeRetencion", Schema = "CNT")]
    public class ChequeRetencion
    {
        [Key]
        public Guid IdDetRetencionCk { get; set; }
        public Guid IdCheque { get; set; }
        public int Index { get; set; }
        public string Retencion { get; set; }
        [Column(TypeName = "decimal(8, 4)")]
        public decimal Porcentaje { get; set; }
        public string Documento { get; set; }
        public string Serie { get; set; }
        public string TipoDocumento { get; set; }
        public string IdMoneda { get; set; }
        [Column(TypeName = "decimal(8, 4)")]
        public decimal TasaCambio { get; set; }
        public decimal Monto { get; set; }
        public decimal MontoMS { get; set; }
        public decimal MontoML { get; set; }
        [Column(TypeName = "decimal(8, 4)")]
        public decimal PorcImpuesto { get; set; }
        public bool TieneImpuesto { get; set; }
        public string CuentaContable { get; set; }
    }
}
