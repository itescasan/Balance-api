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
        public string CuentaC { get; set; }
        public string CuentaD { get; set; }
    }
}
