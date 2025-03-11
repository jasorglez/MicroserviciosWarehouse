using Microsoft.EntityFrameworkCore;
using Warehouse.Models;

namespace Warehouse;

    public partial class DbWarehouseContext : DbContext
    {
        public DbWarehouseContext()
        { }

        public DbWarehouseContext(DbContextOptions<DbWarehouseContext> options)
            : base(options)
        { }

        public virtual DbSet<Catalog> Catalogs { get; set; }
        public virtual DbSet<Configuration> Configurations { get; set; }
        public virtual DbSet<Detailsreqoc> Detailsreqoc { get; set; }        
        public virtual DbSet<Inandout> Inandouts { get; set; }
        public virtual DbSet<Detailsinandout> Detailsinandout { get; set; }
        public virtual DbSet<Material> Materials { get; set; }
        public virtual DbSet<Ocandreq> Ocandreqs { get; set; }
        public virtual DbSet<ProcessXPermission> ProcessXPermissions { get; set; }
        public virtual DbSet<Setup> Setups { get; set; }
        public virtual DbSet<Warehouset> Warehouses { get; set; }
}
