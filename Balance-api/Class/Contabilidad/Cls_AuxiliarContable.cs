using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Balance_api.Class.Contabilidad
{

    [Keyless]
    public class Cls_AuxiliarContable
    {

        public DateTime? Fecha { get; set; }
        public string? Serie { get; set; }
        public string? NoDoc { get; set; }
        public string? Cuenta { get; set; }
        public string? Concepto { get; set; }
        public string? Referencia { get; set; }
        public decimal? DEBE { get; set; }
        public decimal? HABER { get; set; }
        public int? Linea { get; set; }
        public string? Cuenta_Padre { get; set; }
    }
}
