using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Sistema
{
    [Table("MovimientoDoc", Schema = "SIS")]
    public class MovimientoDoc
    {
        [Key]
        public int IdMov { get; set; }
        public string CodigoCliente { get; set; }
        public string NoDocOrigen { get; set; }
        public string SerieOrigen { get; set; }
        public DateTime FechaDocumento { get; set; }
        public int Plazo { get; set; }
        public string TipoVenta { get; set; }
        public decimal TasaCambio { get; set; }
        public string TipoDocumentoOrigen { get; set; }
        public string? NoDocEnlace { get; set; }
        public string? SerieEnlace { get; set; }
        public string? TipoDocumentoEnlace { get; set; }
        public string IdMoneda { get; set; }
        public decimal Total { get; set; }
        public decimal TotalDolar { get; set; }
        public decimal TotalCordoba { get; set; }
        public bool Activo { get; set; }
        public string Esquema { get; set; }

    }
}
