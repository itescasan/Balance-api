using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Sistema
{
    [Table("BodegaSerie", Schema = "SIS")]
    public class BodegaSerie
    {
        [Key]
        public int IdBodegaSerie { get; set; }
        public string CodBodega { get; set; }
        public string Serie { get; set; }
        public string CodUnion { get; set; }
        public string SerieUnion { get; set; }
        public bool EsFact { get; set; }
        public bool EsColector { get; set; }
        public bool EsPedido { get; set; }
        public bool EsCaja { get; set; }
        public bool EsRequisa { get; set; }
        public bool EsInv { get; set; }
        public bool EsExport { get; set; }
        public bool EsImport { get; set; }
        public bool EsPlaneacion { get; set; }
        public bool EsConsignacion { get; set; }
    }
}
