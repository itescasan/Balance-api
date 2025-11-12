using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Nomina
{
    public class Empleado
    {
        [Table("Empleados", Schema = "NOM")]
        public class Empleados
        {
            [Key]
            public int IdEmpleado { get; set; }
            public string? NEmpleado { get; set; }
            public string? NombreCompleto { get; set; }
            public string? CuentaEmpBa { get; set; }
            public bool? Activo { get; set; }


        }
    }
}
