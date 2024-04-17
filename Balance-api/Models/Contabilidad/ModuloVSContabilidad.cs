
using System.ComponentModel.DataAnnotations;


namespace Balance_api.Models.Contabilidad
{
    public class ModuloVSContabilidad
    {
        [Key]
        public Int64 NoLinea { get; set; }
        public string CuentaContable { get; set; }
        public string NoDocumento { get; set; }
        public string CodigoBodega { get; set; }
        public decimal Modulo { get; set; }
        public decimal Contabilidad { get; set; }
        public decimal Saldo { get; set; }
        public string CodConfig { get; set; }
        public string TipoDoc { get; set; }
        public string Tabla { get; set; }

    }
}
