using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Http.Headers;
namespace Balance_api.Models.Contabilidad
{
    [Table("CuentasASociadasCC", Schema = "CNT")]
    public class CuentasAsociadas
    {
        [Key]
        public int Id { get; set; }
        public string CuentaPadre { get; set; }
        public string CuentaAsociada {  get; set; }

    }
}
