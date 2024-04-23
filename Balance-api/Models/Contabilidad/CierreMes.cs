using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Balance_api.Models.Contabilidad
{
    public class CierreMes 
    {
        [Key]
        public int p_Retorno { get; set; }
        public string p_Mensaje { get; set; }
    }
}
