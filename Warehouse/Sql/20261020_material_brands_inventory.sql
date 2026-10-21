/* Asignación opcional de marcas del catálogo a materiales y su captura en movimientos.
   catalog (type = BRAND) es el catálogo maestro. Los saldos históricos conservan id_brand NULL. */
IF OBJECT_ID(N'dbo.materialbrands', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.materialbrands
    (
        id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_materialbrands PRIMARY KEY,
        id_material INT NOT NULL,
        id_catalog INT NOT NULL,
        active BIT NOT NULL CONSTRAINT DF_materialbrands_active DEFAULT (1),
        CONSTRAINT FK_materialbrands_materials FOREIGN KEY (id_material) REFERENCES dbo.materials(id),
        CONSTRAINT FK_materialbrands_catalog FOREIGN KEY (id_catalog) REFERENCES dbo.catalog(id)
    );
END;

IF COL_LENGTH(N'dbo.materialbrands', N'id_catalog') IS NULL
BEGIN
    ;THROW 51001, 'dbo.materialbrands existe con el esquema anterior. Migre sus marcas de texto a dbo.catalog (type BRAND) antes de continuar.', 1;
END;

IF COL_LENGTH(N'dbo.Detailsinandout', N'id_brand') IS NULL
    ALTER TABLE dbo.Detailsinandout ADD id_brand INT NULL;

IF COL_LENGTH(N'dbo.Detailsinandout', N'brand_name') IS NULL
    ALTER TABLE dbo.Detailsinandout ADD brand_name NVARCHAR(120) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.materialbrands') AND name = N'UX_materialbrands_active_material_catalog')
    CREATE UNIQUE INDEX UX_materialbrands_active_material_catalog
        ON dbo.materialbrands(id_material, id_catalog)
        WHERE active = 1;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.materialbrands') AND name = N'IX_materialbrands_material_active')
    CREATE INDEX IX_materialbrands_material_active
        ON dbo.materialbrands(id_material, active);

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Detailsinandout_materialbrands')
    ALTER TABLE dbo.Detailsinandout DROP CONSTRAINT FK_Detailsinandout_materialbrands;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Detailsinandout_catalog_brand')
    ALTER TABLE dbo.Detailsinandout WITH CHECK
        ADD CONSTRAINT FK_Detailsinandout_catalog_brand FOREIGN KEY (id_brand) REFERENCES dbo.catalog(id);
