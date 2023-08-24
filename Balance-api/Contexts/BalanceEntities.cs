using Balance_api.Models.Contabilidad;
using Microsoft.EntityFrameworkCore;

namespace Balance_api.Contexts
{
    public class BalanceEntities : DbContext
    {
        public BalanceEntities(DbContextOptions<BalanceEntities> options) : base(options)
        {

        }

        public DbSet<CatalogoCuenta> CatalogoCuenta { get; set; }
        public DbSet<GruposCuentas> GruposCuentas { get; set; }
    }
}
