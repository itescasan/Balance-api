using Balance_api.Controllers.Contabilidad;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Balance_api.Models.Contabilidad
{
    [Table("InformesContables", Schema = "CNT")]    
    
        public class InformesContables
        {
        [Key]
        public int IdCatalogoInf {  get; set; }
        public string Cuenta { get; set; }
        public int IdTipoInforme { get; set; }
        public int  IdSubGrupo { get; set; }
        public int   Orden { get; set; }
        public bool  UsarNIF { get; set; }
        public string Impuesto { get; set; }
        public string Bodega { get; set; }
    }
 }
