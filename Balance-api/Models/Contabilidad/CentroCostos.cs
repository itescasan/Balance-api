using Balance_api.Models.Banca;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Balance_api.Models.Contabilidad
{
    [Table("CentroCostos", Schema = "CNT")]
    public class CentroCostos
    {
        [Key]
        public int IdCentroCosto { get; set; }
        public string Codigo { get; set; }
        public string CentroCosto { get; set; }
    }
}
