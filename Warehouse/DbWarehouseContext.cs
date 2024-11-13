using Microsoft.EntityFrameworkCore;
using Warehouse.Models;

namespace Warehouse
{
    public partial class DbWarehouseContext : DbContext
    {
        public DbWarehouseContext()
        { }

        public DbWarehouseContext(DbContextOptions<DbWarehouseContext> options)
            : base(options)
        { }

        public virtual DbSet<Warehouset> Warehouses { get; set; }
        public virtual DbSet<Catalog> Catalogs { get; set; }
    }
}
