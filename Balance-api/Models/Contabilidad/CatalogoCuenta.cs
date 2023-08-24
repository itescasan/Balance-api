using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Contabilidad
{
    [Table("CatalogoCuenta", Schema = "CNT")]
    public class CatalogoCuenta
    {
        [Key]
        public required string CuentaContable { get; set; }
        public required string NombreCuenta { get; set; }
        public int Nivel { get; set; }
        public int? GrupoCuentas { get; set; }
        public string? ClaseCuenta { get; set; }
        public string? CuentaPadre { get; set; }
        public required string Naturaleza { get; set; }
        public bool Bloqueada { get; set; }
        public int? IdUsuarioReg { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaUpdate { get; set; }
        public int? IdUsuarioModifica { get; set; }

    }
}
