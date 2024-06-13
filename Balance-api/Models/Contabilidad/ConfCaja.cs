using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Balance_api.Models.Contabilidad
{
    [Table("ConfCajaChica", Schema = "CNT")]
    public class ConfCaja
    {
        [Key]
        public int IdTecho { get; set; }
        public string CuentaContable { get; set; }
        public string Nombre { get; set; }
        public decimal Valor { get; set; }
        public string Estado { get; set; }
        public string Serie {  get; set; }
        public int Consecutivo {  get; set; }
        
    }
}
