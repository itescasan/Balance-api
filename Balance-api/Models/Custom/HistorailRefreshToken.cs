using Balance_api.Models.Contabilidad;
using Balance_api.Models.Sistema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Balance_api.Models.Custom
{

    [Table("HistorailRefreshToken", Schema = "SIS")]
    public class HistorailRefreshToken
    {
        [Key]
        public int IdHistorialToken { get; set; }
        public int IdUsuario { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaExpiracion { get; set; }


        [ForeignKey("IdUsuario")]
        public ICollection<Usuarios> Usuarios { get; set; }

    }
}
