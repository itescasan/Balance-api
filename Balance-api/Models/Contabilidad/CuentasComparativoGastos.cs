using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Balance_api.Models.Contabilidad
{
    [Table("CuentasComparativoGastos", Schema = "CNT")]
    public class CuentasComparativoGastos
    {
        [Key]
        public int IdReporteAcumulado { get; set; }
        public string CuentaGastosAdmon { get; set; }
        public string CuentaGastosVentas { get; set; }
        public string NombreCuenta { get; set; }        
        public bool Activo { get; set; }
    }
}
