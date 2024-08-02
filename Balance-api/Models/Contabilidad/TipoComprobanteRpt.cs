using Balance_api.Controllers.Contabilidad;
using DevExpress.XtraRichEdit.Export.Rtf;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Contabilidad
{
    [Table("TipoComprobanteRpt", Schema = "CNT")]
    public class TipoComprobanteRpt
    {
        [Key]
        public int IdTipoComprobanterpt { get; set; }        
        public string IdSerie { get; set; }
        public string TipoComprobante { get; set; }
        
    }
}
