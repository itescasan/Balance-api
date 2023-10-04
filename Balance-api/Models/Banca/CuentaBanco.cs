using Balance_api.Models.Contabilidad;
using Balance_api.Models.Sistema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Banca
{
    [Table("CuentaBanco", Schema = "BAN")]
    public class CuentaBanco
    {
        [Key]
        public int IdCuentaBanco { get; set; }
        public int IdBanco { get; set; }
        public string CuentaBancaria { get; set; }
        public string NombreCuenta { get; set; }
        public string IdMoneda { get; set; }

        public int ConsecutivoCheque { get; set; }
        public int ConsecutivoTransferencia { get; set; }
        public bool Activo { get; set; }

        [ForeignKey("IdBanco")]
        public Bancos Bancos { get; set; }

        [ForeignKey("IdMoneda")]
        public Monedas Monedas { get; set; }
    }
}
