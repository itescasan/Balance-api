using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Contabilidad
{
    [Table("ClientesImpuestoAlcaldia", Schema = "CXC")]
    public class VentasClientesAlcaldia
    {
        [Key]
        public int idClienteImpAl {  get; set; }
        
        public string CodCliente { get; set; }  
        
        public string Municipio { get; set; }
        
        public string OrigenVenta { get; set; }
        
        public string Estado { get; set; }
    }
}
