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
using Balance_api.Models.Custom;
namespace Balance_api.Contexts
{
    public class BalanceEntities : DbContext
    {
        public BalanceEntities(DbContextOptions<BalanceEntities> options) : base(options)
        {
           
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(options => options.CommandTimeout(30)); // Timeout de consulta en segundos);

        }


        public BalanceEntities()
        {
            
        }
           

        protected override void OnModelCreating(ModelBuilder modelBuilder)
         {

            //CUENTAS CONTABLES
            modelBuilder.Entity<CatalogoCuenta>();
            modelBuilder.Entity<CatalogoCuenta>().ToTable(tb => tb.HasTrigger("CNT.TR_AUDITORIA_CNT_CatalogoCuenta"));




            //ASIENTO CONTABLE

            modelBuilder.Entity<Asiento>()
            .HasMany(e => e.AsientosContablesDetalle)
            .WithOne(e => e.Asiento)
            .HasForeignKey(e => e.IdAsiento);
            modelBuilder.Entity<Asiento>().ToTable(tb => tb.HasTrigger("CNT.TR_AUDITORIA_CNT_AsientosContables"));
            modelBuilder.Entity<AsientoDetalle>().ToTable(tb => tb.HasTrigger("CNT.TR_AUDITORIA_CNT_AsientosContablesDetalle"));
            //FIN



            //GURPO CUENTAS
            modelBuilder.Entity<GruposCuentas>()
                 .HasMany(e => e.CatalogoCuenta)
                 .WithOne(e => e.GruposCuentas)
                 .HasForeignKey(e => e.IdGrupo);
           


            //SERIES
            modelBuilder.Entity<SerieDocumento>()
                .HasOne(e => e.TipoDocumento)
                .WithMany()
                .HasForeignKey(e => e.IdTipoDocumento);
            //







            //TRANSFERENCIA
            modelBuilder.Entity<Transferencia>()
               .HasMany(e => e.TransferenciaDocumento)
               .WithOne(e => e.Transferencia)
               .HasForeignKey(e => e.IdTransferencia);

            modelBuilder.Entity<Transferencia>()
              .HasMany(e => e.TranferenciaRetencion)
              .WithOne(e => e.Transferencia)
              .HasForeignKey(e => e.IdTransferencia);

            modelBuilder.Entity<Transferencia>().ToTable(tb => tb.HasTrigger("CNT.TR_AUDITORIA_CNT_Transferencia"));
            //FIM


            //CHEQUES
            modelBuilder.Entity<Cheques>()
               .HasMany(e => e.ChequeDocumento)
               .WithOne(e => e.cheques)
               .HasForeignKey(e => e.IdCheque);

            modelBuilder.Entity<Cheques>()
              .HasMany(e => e.ChequeRetencion)
              .WithOne(e => e.cheques)
              .HasForeignKey(e => e.IdCheque); 

            modelBuilder.Entity<Cheques>().ToTable(tb => tb.HasTrigger("CNT.TR_AUDITORIA_CNT_Cheques"));
            //FIM






            //MOVIMIENTO DOC
            modelBuilder.Entity<MovimientoDoc>().ToTable(tb => tb.HasTrigger("SIS.TR_AUDITORIA_SIS_MovimientoDoc"));
            //FIN




        }






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

        //public virtual DbSet<CentroCostos> CentroCostos { get; set; }

        public virtual DbSet<CatalogoCentroCostos> CatalogoCentroCostos { get; set; }

        public virtual DbSet<ChequeRetencion> ChequeRetencion { get; set; }
        public virtual DbSet<CierreMes> CierreMes { get; set; }

        public virtual DbSet<ModuloVSContabilidad> ModuloVSContabilidad { get; set; }

        public DbSet<AccesoCajaC> AccesoCajaChica { get; set; }

        public DbSet<ConfCaja> ConfCaja { get; set; }

        public DbSet<IngresoCaja> IngresoC { get; set; } 

        public DbSet<DetalleIngresoCaja> DetIngCaja { get; set; }

        public DbSet<VentasClientesAlcaldia> VentasClientesAlcaldia { get; set; } 

        public DbSet<TipoComprobanteRpt> TipoComprobanteRep { get; set; }

        public DbSet<CuentasComparativoGastos> CuentasComparativoGastos { get; set; }
        public DbSet<CuentasAsociadas> CuentasAsociadas { get; set; }
        public DbSet<CajasAsociadas> CajasAsociadas { get; set; }





        //██████████████████████████████████████████INVENTARIO████████████████████████████████████████████████████████
        public DbSet<Bodegas> Bodegas { get; set; }





        //██████████████████████████████████████████BANCA█████████████████████████████████████████████████████████████
        public DbSet<Bancos> Bancos { get; set; }
        public DbSet<CuentaBanco> CuentaBanco { get; set; }



        //██████████████████████████████████████████PROVEEDOR████████████████████████████████████████████████████████
        public DbSet<Proveedor> Proveedor { get; set; }
        public DbSet<Retenciones> Retenciones { get; set; }
        public DbSet<CatalogoGastosInternos> CatalogoGastosInternos { get; set; }
        public DbSet<OrdenCompra> OrdenCompra { get; set; }
        public DbSet<CuentaXPagar> CuentaXPagar { get; set; }
        public DbSet<OrdenCompraCentrogasto> OrdenCompraCentrogasto { get; set; }
        public DbSet<Articulo> Articulos { get; set; }
        
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

        public DbSet<HistorailRefreshToken> HistorailRefreshToken { get; set; }

    }

    


}
