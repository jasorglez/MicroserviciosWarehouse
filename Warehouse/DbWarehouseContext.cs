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
    public virtual DbSet<TablesXModules> TablesXModules { get; set; }
    public virtual DbSet<Warehouset> Warehouses { get; set; }
    public virtual DbSet<RawMaterial> RawMaterial { get; set; }
    public virtual DbSet<RawMaterialDetails> RawMaterialDetails { get; set; }
    public virtual DbSet<PricesXProductsPresentation> PricesXProductsPresentation { get; set; }
    public virtual DbSet<MaterialsByBranchVW> MaterialsByBranchVW { get; set; }
    public virtual DbSet<MaterialsxProvExist> MaterialsxProvExists { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)

    {
        modelBuilder.Entity<MaterialsByBranchVW>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("materialsByBranchVW"); // 👈 nombre de la vista en la DB
            });
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MaterialsxProvExist>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("MaterialsxProvExist"); // 👈 nombre de la vista en la DB
        });
        base.OnModelCreating(modelBuilder);


    }
}
