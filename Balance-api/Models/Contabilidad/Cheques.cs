﻿using Balance_api.Models.Banca;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_api.Models.Contabilidad
{
    [Table("Cheques", Schema = "CNT")]
    public class Cheques
    {
        public Cheques()
        {
            this.ChequeDocumento = new HashSet<ChequeDocumento>();
            this.ChequeRetencion = new HashSet<ChequeRetencion>();
        }

        [Key]
        public Guid IdCheque { get; set; }
        public int IdCuentaBanco { get; set; }
        public string? CuentaContable { get; set; }
        public string? CentroCosto { get; set; }
        public string IdMoneda { get; set; }       
        public string CodBodega { get; set; }
        public string IdSerie { get; set; }
        public string NoCheque { get; set; }
        public DateTime Fecha { get; set; }
        public string Beneficiario { get; set; }
        public string? CodProveedor { get; set; }
        [Column(TypeName = "decimal(8, 4)")]
        public decimal TasaCambio { get; set; }
        public string Concepto { get; set; }
        public string TipoCheque { get; set; }
        public decimal Comision { get; set; }
        public decimal ComisionDolar { get; set; }
        public decimal ComisionCordoba { get; set; }
        public decimal Total { get; set; }
        public decimal TotalDolar { get; set; }
        public decimal TotalCordoba { get; set; }
        public bool Anulado { get; set; }

        public string UsuarioReg { get; set; }
        public DateTime FechaReg { get; set; }
        public string UsuarioUpdate { get; set; }
        public DateTime FechaUpdate { get; set; } 
        public string? UsuarioAnula { get; set; }
        public DateTime? FechaAnulacion { get; set; }
        public int? IdIngresoCaja { get; set; }
        public string? CuentaIngCaja {get; set; }

        public decimal ValorCheque { get; set; }

        [ForeignKey("IdCuentaBanco")]
        public CuentaBanco CuentaBanco { get; set; }

 
        public ICollection<ChequeDocumento> ChequeDocumento { get; set; } 


        public ICollection<ChequeRetencion> ChequeRetencion { get; set; }
    }
}
