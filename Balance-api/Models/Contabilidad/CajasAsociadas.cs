using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Contabilidad
{

    [Table("CajasAsociadas", Schema = "CNT")]
    public class CajasAsociadas
    {
        [Key]
        public int IdAcceso { get; set; }
        public string CuentaContable { get; set; }
        public string Usuario { get; set; }
        public bool Clase { get; set; }
        public string NombreCuenta { get; set; }      
    }
}

