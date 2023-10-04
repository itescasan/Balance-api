using Balance_api.Models.Banca;
using Balance_api.Models.Contabilidad;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Sistema
{
    [Table("Monedas", Schema = "SIS")]
    public class Monedas
    {
 
        [Key]
        public string IdMoneda { get; set; }
        public string Moneda { get; set; }
        public string Simbolo { get; set; }


    }
}