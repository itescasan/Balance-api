using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Banca
{
    [Table("Bancos", Schema = "BAN")]
    public class Bancos
    {
        [Key]
        public int IdBanco { get; set; }
        public string Banco { get; set; }
        public string CuentaNuevaC { get; set; }
        public string CuentaNuevaD { get; set; } 

    }
}
