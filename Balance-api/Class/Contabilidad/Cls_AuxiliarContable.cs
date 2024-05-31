using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Balance_api.Class.Contabilidad
{

    [Keyless]
    public class Cls_AuxiliarContable
    {
        public int IdAsiento { get; set; }
        public string Modulo { get; set; }
        public DateTime? Fecha { get; set; }
        public string? Serie { get; set; }
        public string? NoDoc { get; set; }
        public string? Cuenta { get; set; }
        public string? Concepto { get; set; }
        public string? Referencia { get; set; }
        public decimal? DEBE_ML { get; set; }
        public decimal? HABER_ML { get; set; }
        public decimal? Saldo_ML { get; set; }
        public decimal? DEBE_MS { get; set; }
        public decimal? HABER_MS { get; set; }
        public decimal? Saldo_MS { get; set; }
        public string? Cuenta_Padre { get; set; }
        public string? Bodega { get; set; }
        public int? Editar { get; set; }
        public int? Linea { get; set; }
       
    }
}
