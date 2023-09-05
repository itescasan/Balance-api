using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Sistema
{
    [Table("Serie", Schema = "SIS")]
    public class Serie
    {
        [Key]
        public string IdSerie { get; set; }
        public int Factura { get; set; }
        public int Recibo { get; set; }
        public int Colector { get; set; }
        public int Inventario { get; set; }
        public int Ticket { get; set; }
        public int Pedido { get; set; }
        public int Requisa { get; set; }
        public int Proforma { get; set; }
        public int Garantia { get; set; }
        public int OT { get; set; }
        public int ProformaProv { get; set; }
        public int Remision { get; set; }
        public int Abaste { get; set; }
        public int Anexos { get; set; }
    }
}
