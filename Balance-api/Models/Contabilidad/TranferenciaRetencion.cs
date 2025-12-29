using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Contabilidad
{
    [Table("TransferenciaRetencion", Schema = "CNT")]
    public class TranferenciaRetencion
    {
        [Key]
        public Guid IdDetRetencion { get; set; }
        public Guid IdTransferencia { get; set; }
        public int Index { get; set; }
        public int IdRetencion { get; set; }
        public string Retencion { get; set; }
        [Column(TypeName = "decimal(8, 4)")]
        public decimal Porcentaje { get; set; }
        public string Documento { get; set; }
        public string Serie { get; set; }
        public string TipoDocumento { get; set; }
        public string IdMoneda { get; set; }
        [Column(TypeName = "decimal(8, 4)")]
        public decimal TasaCambio { get; set; }
        public decimal SubTotal { get; set; }
        public decimal SubTotalMS { get; set; }
        public decimal SubTotalML { get; set; }
        public decimal Monto { get; set; }
        public decimal MontoMS { get; set; }
        public decimal MontoML { get; set; }
        [Column(TypeName = "decimal(8, 4)")]
        public decimal PorcImpuesto { get; set; }
        public bool TieneImpuesto { get; set; }
        public string CuentaContable { get; set; }

        public bool RetManual { get; set; }
        public string Naturaleza { get; set; }
        public bool AplicarAutomatico { get; set; }

        [ForeignKey("IdTransferencia")]
        public Transferencia Transferencia { get; set; }

    }
}
