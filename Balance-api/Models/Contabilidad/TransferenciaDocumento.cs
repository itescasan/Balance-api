namespace Balance_api.Models.Contabilidad
{
    public class TransferenciaDocumento
    {
        public int IdDetTrasnfDoc { get; set; }
        public int Index { get; set; }
        public string Operacion { get; set; }
        public string Documento { get; set; }
        public string Serie { get; set; }
        public string TipoDocumento { get; set; }
        public DateTime Fecha { get; set; }
        public string IdMoneda { get; set; }
        public decimal TasaCambioDoc { get; set; }
        public decimal Saldo { get; set; }
        public decimal SaldoDolar { get; set; }
        public decimal SaldoCordoba { get; set; }
        public decimal Importe { get; set; }
        public decimal NuevoSaldo { get; set; }
 
    }
}
