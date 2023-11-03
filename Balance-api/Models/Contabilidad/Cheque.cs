using Balance_api.Models.Banca;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Contabilidad
{
    [Table("Cheque", Schema = "CNT")]
    public class Cheque
    {
        [Key]
        public Guid IdTransferencia { get; set; }
        public int IdCuentaBanco { get; set; }
        public string CuentaContable { get; set; }
        public string CodBodega { get; set; }
        public string IdSerie { get; set; }
        public string NoTransferencia { get; set; }
        public DateTime Fecha { get; set; }
        public string Beneficiario { get; set; }
        public decimal TasaCambio { get; set; }
        public string Concepto { get; set; }
        public string TipoTransferencia { get; set; }
        public decimal Total { get; set; }
        public decimal TotalDolar { get; set; }
        public decimal TotalCordoba { get; set; }
        public bool Anulado { get; set; }

        public string UsuarioReg { get; set; }
        public DateTime FechaReg { get; set; }
        public string UsuarioUpdate { get; set; }
        public DateTime FechaUpdate { get; set; }
        public string? UsuarioAnula { get; set; }
        public DateTime? FechaAnulacion { get; set; }


        [ForeignKey("IdCuentaBanco")]
        public CuentaBanco CuentaBanco { get; set; }

        [ForeignKey("CuentaContable")]
        public CatalogoCuenta CatalogoCuenta { get; set; }
    }
}
