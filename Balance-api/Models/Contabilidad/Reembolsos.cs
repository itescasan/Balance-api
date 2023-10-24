using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Balance_api.Models.Contabilidad
{
      public class Reembolsos
    {
        [Key]
        public int Id { get; set; }
        public string? Ccosto { get; set; } 
       // public string Fecha { get; set; }
        public string? Numero { get; set; }
        /*public string Concepto { get; set; }
        public decimal Valor { get; set; }
        public string Referencia { get; set; }
        public string Cuenta { get; set; }
        public string FechaTran { get; set; }
        public string Proveedor { get; set; }
        public bool Aplicado { get; set; }
        public string? Tipo { get; set; }
        public bool Contabilizado { get; set; }
        public int IdCC { get; set; }
        public string CentroC { get; set; }
        public string Nemp { get; set; }
        public string Empleado { get; set; }
        public string Usuario { get; set; }*/
    }
}
