/* Marca opcional por partida de requisición u orden de compra.
   Ejecutar después de 20261020_material_brands_inventory.sql. */
IF COL_LENGTH(N'dbo.detailsreqoc', N'id_brand') IS NULL
    ALTER TABLE dbo.detailsreqoc ADD id_brand INT NULL;

IF COL_LENGTH(N'dbo.detailsreqoc', N'brand_name') IS NULL
    ALTER TABLE dbo.detailsreqoc ADD brand_name NVARCHAR(120) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_detailsreqoc_catalog_brand')
    ALTER TABLE dbo.detailsreqoc WITH CHECK
        ADD CONSTRAINT FK_detailsreqoc_catalog_brand FOREIGN KEY (id_brand) REFERENCES dbo.catalog(id);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.detailsreqoc') AND name = N'IX_detailsreqoc_material_brand')
    CREATE INDEX IX_detailsreqoc_material_brand ON dbo.detailsreqoc(id_supplie, id_brand);
