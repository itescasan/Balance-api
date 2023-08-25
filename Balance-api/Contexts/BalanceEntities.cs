using Balance_api.Models.Contabilidad;
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


        public DbSet<CatalogoCuenta> CatalogoCuenta { get; set; }
        public DbSet<GruposCuentas> GruposCuentas { get; set; }


     

    }

    


}
