using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Contabilidad
{
    [Table("Periodos", Schema = "CNT")]
    public class Periodos
    {
        [Key]        
        public int? IdPeriodo { get; set; }
        public int? IdEjercicio { get; set; }
        public int? NoPeriodo { get; set; }
        public string? NombrePeriodo { get; set; }
        public string? ClasePeriodo { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFinal { get; set; }
        public string? Estado { get; set; }
        public DateTime? FechaReg { get; set; }
        public string UsuarioReg { get; set; }
        public DateTime FechaUpdate { get; set; }
        public string UsuarioUpdate { get; set; }


        //[ForeignKey("IdEjercicio")]
        //public ICollection<EjercicioFiscal> EjercicioFiscal { get; set; }

    }
}
