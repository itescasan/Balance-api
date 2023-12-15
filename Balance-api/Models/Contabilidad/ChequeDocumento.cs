using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Contabilidad
{
    [Table("ChequeDocumento", Schema = "CNT")]
    public class ChequeDocumento
    {
        [Key]
        public Guid IdDetChequeDoc { get; set; }
        public Guid IdCheque { get; set; }
        public int Index { get; set; }
        public string Operacion { get; set; }
        public string Documento { get; set; }
        public string Serie { get; set; }
        public string TipoDocumento { get; set; }
        public DateTime Fecha { get; set; }
        public string IdMoneda { get; set; }
        [Column(TypeName = "decimal(8, 4)")]
        public decimal TasaCambioDoc { get; set; }
        public decimal SaldoAnt { get; set; }
        public decimal SaldoAntML { get; set; }
        public decimal SaldoAntMS { get; set; }
        public decimal Saldo { get; set; }
        public decimal SaldoDolar { get; set; }
        public decimal SaldoCordoba { get; set; }
        public decimal Importe { get; set; }
        public decimal ImporteML { get; set; }
        public decimal ImporteMS { get; set; }
        public decimal NuevoSaldo { get; set; }
        public decimal NuevoSaldoML { get; set; }
        public decimal NuevoSaldoMS { get; set; }
        public decimal DiferencialML { get; set; }
        public decimal DiferencialMS { get; set; }
        public bool Retenido { get; set; }
    }
}
