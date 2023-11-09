using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Sistema
{
    [Table("MovimientoDoc", Schema = "SIS")]
    public class MovimientoDoc
    {
        [Key]
        public int IdMov { get; set; }
        public string NoMovimiento { get; set; }
        public int IdBodega { get; set; }
        public string? CodigoBodega { get; set; }
        public string CodigoCliente { get; set; }
        public string CodVendedor { get; set; }
        public string NoDocOrigen { get; set; }
        public string SerieOrigen { get; set; }
        public DateTime FechaDocumento { get; set; }
        public int Plazo { get; set; }
        public int DiaGracia { get; set; }
        public string TipoVenta { get; set; }
        public decimal TasaCambio { get; set; }
        public string TipoDocumentoOrigen { get; set; }
        public string? NoDocEnlace { get; set; }
        public string? SerieEnlace { get; set; }
        public string? TipoDocumentoEnlace { get; set; }
        public string IdMoneda { get; set; }
        public decimal SubTotal { get; set; }
        public decimal SubTotalDolar { get; set; }
        public decimal SubTotalCordoba { get; set; }
        public decimal Descuento { get; set; }
        public decimal DescuentoDolar { get; set; }
        public decimal DescuentoCordoba { get; set; }
        public decimal SubTotalNeto { get; set; }
        public decimal SubTotalNetoDolar { get; set; }
        public decimal SubTotalNetoCordoba { get; set; }
        public decimal Impuesto { get; set; }
        public decimal ImpuestoDolar { get; set; }
        public decimal ImpuestoCordoba { get; set; }
        public decimal Total { get; set; }
        public decimal TotalDolar { get; set; }
        public decimal TotalCordoba { get; set; }
        public decimal DiferencialDolar { get; set; }
        public decimal DiferencialCordoba { get; set; }
        public decimal RetenidoAlma { get; set; }
        public decimal RetenidoAlmaDolar { get; set; }
        public decimal RetenidoAlmaCordoba { get; set; }
        public decimal RetenidoIR { get; set; }
        public decimal RetenidoIRDolar { get; set; }
        public decimal RetenidoIRCordoba { get; set; }
        public string Operacion { get; set; }
        public bool Activo { get; set; }
        public string Esquema { get; set; }

    }
}
