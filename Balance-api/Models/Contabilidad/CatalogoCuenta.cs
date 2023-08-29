using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Contabilidad
{
    [Table("CatalogoCuenta", Schema = "CNT")]
    public class CatalogoCuenta
    {

        [Key]
        public  string? CuentaContable { get; set; }
        public  string? NombreCuenta { get; set; }
        public int Nivel { get; set; }
        public int IdGrupo { get; set; }
        public string? ClaseCuenta { get; set; }
        public string? CuentaPadre { get; set; }
        public  string? Naturaleza { get; set; }
        public bool Bloqueada { get; set; }
        public string? UsuarioReg { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaUpdate { get; set; }
        public string? UsuarioModifica { get; set; }

        [ForeignKey("IdGrupo")]
        public   GruposCuentas? GruposCuentas { get; set; }

    }
}
