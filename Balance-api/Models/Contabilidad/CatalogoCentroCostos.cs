using Balance_api.Models.Banca;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Balance_api.Models.Contabilidad
{

    [Table("CatalogoCentroCostos", Schema = "CNT")]
    public class CatalogoCentroCostos
    {
        [Key]
        public int IdCatalogoCentroCosto { get; set; }
        public string Codigo { get; set; }
        public string CentroCosto { get; set; }
    }
}
