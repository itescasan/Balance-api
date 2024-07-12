using Balance_api.Controllers.Contabilidad;
using DevExpress.XtraRichEdit.Export.Rtf;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Balance_api.Models.Contabilidad
{
    [Table("DetalleIngresoCajaChica", Schema = "CNT")]
    public class DetalleIngresoCaja
    {
        [Key]
        public int IdDetalleIngresoCajaChica { get; set; }
        public int IdIngresoCajaC { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime FechaFactura { get; set; }
        public string Concepto { get; set; }
        public string Referencia { get; set; }
        public string Proveedor { get; set; }
        public string Cuenta { get; set; }
        public string CentroCosto { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }
        public string CuentaEmpleado { get; set; }       
        public DateTime FechaModificacion { get; set; }
    }
}

















