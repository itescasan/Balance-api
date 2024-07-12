using Balance_api.Controllers.Contabilidad;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Balance_api.Models.Contabilidad
{
    [Table("IngresosCajaChica", Schema = "CNT")]
    public class IngresoCaja
    {
        [Key]
        public int IdIngresoCajaChica { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Cuenta { get; set; }
        public int Consecutivo { get; set; }
        public DateTime FechaModificacion { get; set; } 
        public string Usuario { get; set; }
        public string UsuarioModifica { get; set; }
        public Boolean Aplicado { get; set; }
        public Boolean Contabilizado { get; set; }

        [ForeignKey("IdIngresoCajaC")]
        public ICollection<DetalleIngresoCaja> DetalleCaja { get; set; } 
    }
}


