using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Balance_api.Models.Contabilidad
{
    [Table("AccesoCajaChica", Schema = "CNT")]
    public class AccesoCajaC
    {
        [Key]
        public int IdAcceso { get; set; }
        public string CuentaContable { get; set; }  
        public string Usuario { get; set; }        
        public bool Activo { get; set; }            
    }
}
