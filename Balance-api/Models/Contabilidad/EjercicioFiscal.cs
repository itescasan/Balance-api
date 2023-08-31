using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Contabilidad
{
    [Table("EjercicioFiscal", Schema = "CNT")]
    public class EjercicioFiscal
    {
        [Key]
        public int? IdEjercicio { get; set; }
        public string? Nombre { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFinal { get; set; }
        public string? ClasePeriodos { get; set; }
        public int? NumerosPeriodos { get; set; }
        public string? CuentaContableAcumulada { get; set; }
        public string? CuentaPerdidaGanancia { get; set; }
        public string? CuentaContablePeriodo { get; set; }
        public DateTime? FechaReg { get; set; }
        public int? IdUsuarioReg { get; set; }
        public DateTime FechaUpdate { get; set; }
        public int IdUsuarioUpdate { get; set; }

        [ForeignKey("IdEjercicio")]
        public Periodos? Periodos { get; set; }
    }
}
