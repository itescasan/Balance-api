using Balance_api.Class.Contabilidad;
using Balance_api.Controllers.Contabilidad;
using Balance_api.Models.Banca;
using Balance_api.Models.Contabilidad;
using Balance_api.Models.Inventario;
using Balance_api.Models.Proveedor;
using Balance_api.Models.Sistema;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Balance_api.Contexts
{
    public class BalanceEntities : DbContext
    {
        public BalanceEntities(DbContextOptions<BalanceEntities> options) : base(options)
        {

        }


        /* protected override void OnModelCreating(ModelBuilder modelBuilder)
         {

             modelBuilder.Entity<GruposCuentas>()
                 .HasMany(e => e.CatalogoCuenta)
                 .WithOne(e => e.GruposCuentas)
                 .HasForeignKey(e => e.IdGrupo);


         }*/



        //██████████████████████████████████████████CONTABILIDAD██████████████████████████████████████████████████████


        public DbSet<CatalogoCuenta> CatalogoCuenta { get; set; }

        public DbSet<InformesContables> InformesContables { get; set; } 
        public DbSet<GruposCuentas> GruposCuentas { get; set; }

        public DbSet<EjercicioFiscal> EjercicioFiscal { get; set; }
        public DbSet<Periodos> Periodos { get; set; }
        public DbSet<Asiento> AsientosContables { get; set; }
        public DbSet<AsientoDetalle> AsientosContablesDetalle { get; set; }

        public virtual DbSet<Cls_AuxiliarContable> AuxiliarContable { get; set; }
        public virtual DbSet<Transferencia> Transferencia { get; set; }
        public virtual DbSet<TransferenciaDocumento> TransferenciaDocumento { get; set; }
        public virtual DbSet<TranferenciaRetencion> TranferenciaRetencion { get; set; }
        public virtual DbSet<ChequeDocumento> ChequeDocumento { get; set; }
        public virtual DbSet<Reembolsos> Reembolsos { get; set; }

        public virtual DbSet<ReembolsosD> ReembolsosD { get; set; }

        public virtual DbSet<Cheques> Cheque { get; set; }

        public virtual DbSet<CentroCostos> CentroCostos { get; set; }

        public virtual DbSet<ChequeRetencion> ChequeRetencion { get; set; }
        public virtual DbSet<CierreMes> CierreMes { get; set; }

        public virtual DbSet<ModuloVSContabilidad> ModuloVSContabilidad { get; set; }

        public DbSet<AccesoCajaC> AccesoCajaChica { get; set; }

        public DbSet<ConfCaja> ConfCaja { get; set; }

        public DbSet<IngresoCaja> IngresoC { get; set; } 

        public DbSet<DetalleIngresoCaja> DetIngCaja { get; set; }

        public DbSet<VentasClientesAlcaldia> VentasClientesAlcaldia { get; set; } 

        public DbSet<TipoComprobanteRpt> TipoComprobanteRep { get; set; }
        



        //██████████████████████████████████████████INVENTARIO████████████████████████████████████████████████████████
        public DbSet<Bodegas> Bodegas { get; set; }





        //██████████████████████████████████████████BANCA█████████████████████████████████████████████████████████████
        public DbSet<Bancos> Bancos { get; set; }
        public DbSet<CuentaBanco> CuentaBanco { get; set; }



        //██████████████████████████████████████████PROVEEDOR████████████████████████████████████████████████████████
        public DbSet<Proveedor> Proveedor { get; set; }
        public DbSet<Retenciones> Retenciones { get; set; }

        //██████████████████████████████████████████SISTEMA███████████████████████████████████████████████████████████


        public DbSet<Usuarios> Usuarios { get; set; }
        public DbSet<Monedas> Monedas { get; set; }
        public DbSet<Serie> Serie { get; set; }
        public DbSet<BodegaSerie> BodegaSerie { get; set; }
        public DbSet<SerieDocumento> SerieDocumento { get; set; }
        public DbSet<ConsecutivoDiario> ConsecutivoDiario { get; set; }

        public DbSet<TipoDocumento> TipoDocumento { get; set; }
        public DbSet<MovimientoDoc> MovimientoDoc { get; set; }
        public DbSet<AccesoWeb> AccesoWeb { get; set; }

    }

    


}
