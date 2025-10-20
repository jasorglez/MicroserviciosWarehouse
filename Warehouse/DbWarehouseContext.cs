﻿using Microsoft.EntityFrameworkCore;
using Warehouse.Models;
using Warehouse.Models.Delison;
using Warehouse.Models.Views;

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
    public virtual DbSet<ProveedorXTabla> ProveedorXTablas { get; set; }
    public virtual DbSet<ProveedoresxtypeView> ProveedoresxtypeViews { get; set; }
    public virtual DbSet<MaterialsDelison> MaterialsDelison { get; set; }
    public virtual DbSet<CreditProveedores> CreditProveedores { get; set; }
    public virtual DbSet<MaterialsXProvider> MaterialsxProviders { get; set; }
    public virtual DbSet<MaterialWithCount> MaterialWithCounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)

    {        
        modelBuilder.Entity<MaterialWithCount>(entity =>
        {
            entity.HasNoKey(); // Las vistas generalmente no tienen key
            entity.ToView("vw_MaterialsWithCounts");            
            // O si prefieres definir una key:
            // entity.HasKey(e => e.Id);
        });
    
        modelBuilder.Entity<MaterialsxProvExist>(entity =>
         {
            entity.HasNoKey();
            entity.ToView("materialsxproviders"); // 👈 nombre de la vista en la DB
         });
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MaterialsByBranchVW>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("materialsByBranchVW"); // 👈 nombre de la vista en la DB
            });
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProveedoresxtypeView>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("Proveedoresxtype"); // 👈 nombre de la vista en la DB
        });
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MaterialsxProvExist>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("MaterialsxProvExist"); // 👈 nombre de la vista en la DB
        });
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CreditProveedores>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("creditproveedores"); // 👈 nombre de la vista en la DB
        });
        base.OnModelCreating(modelBuilder);


    }
}
