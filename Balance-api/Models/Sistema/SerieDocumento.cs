using Balance_api.Models.Contabilidad;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Sistema
{
    [Table("SerieDocumentos", Schema = "CNT")]
    public class SerieDocumento
    {
        [Key]
        public string IdSerie { get; set; }
        public string DescripcionSerie { get; set; }
        public int IdTipoDocumento { get; set; }
        public int Consecutivo { get; set; }
        public bool Activo { get; set; }



        [ForeignKey("IdTipoDocumento")]
        public TipoDocumento TipoDocumento { get; set; }
    }
}
