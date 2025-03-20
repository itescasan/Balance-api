using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Contabilidad
{
    [Table("AsientosContables", Schema = "CNT")]
    public class Asiento
    {

        public Asiento()
        {
            this.AsientosContablesDetalle = new HashSet<AsientoDetalle>();
        }



        [Key]
        public int IdAsiento { get; set; }
        public int IdPeriodo { get; set; }
        public string NoAsiento { get; set; }
        public string IdSerie { get; set; }
        public DateTime Fecha { get; set; }
        public string IdMoneda { get; set; }
        [Column(TypeName = "decimal(8, 4)")]
        public decimal TasaCambio { get; set; }
        public string Concepto { get; set; }
        public string? NoDocOrigen { get; set; }
        public string? IdSerieDocOrigen { get; set; }
        public string? TipoDocOrigen { get; set; }
        public string? Bodega { get; set; }
        public string Referencia { get; set; }
        public string Estado { get; set; }
        public string TipoAsiento { get; set; }
        public decimal? Total { get; set; }
        public decimal TotalML { get; set; }
        public decimal TotalMS { get; set; }
        public DateTime FechaReg { get; set; }
        public string UsuarioReg { get; set; }
        public DateTime? FechaUpdate { get; set; }
        public string? UsuarioUpdate { get; set; }
        public bool? Automatico { get; set; }
        public bool? Revisado { get; set; }

        public ICollection<AsientoDetalle> AsientosContablesDetalle { get; set; }



    }
}
