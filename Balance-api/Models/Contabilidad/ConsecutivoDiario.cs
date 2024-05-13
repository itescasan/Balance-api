using Balance_api.Models.Sistema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Balance_api.Models.Contabilidad
{
    [Table("ConsecutivoDiario", Schema = "CNT")]
    public class ConsecutivoDiario
    {
        [Key]
        public int IdConDia { get; set; }
        public string IdSerie { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }
        public int Consecutivo { get; set; }

        [ForeignKey("IdSerie")]
        public SerieDocumento SerieDocumento { get; set; }
    }
}
