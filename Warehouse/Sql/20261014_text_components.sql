USE warehouses;
GO

/* Convierte las relaciones a componentes de texto para bases que ya ejecutaron
   20261013_material_components.sql. Conserva las definiciones existentes. */
IF COL_LENGTH(N'dbo.materialcomponents', N'component_name') IS NULL
    ALTER TABLE dbo.materialcomponents ADD component_name NVARCHAR(200) NULL;
GO

IF COL_LENGTH(N'dbo.materialcomponents', N'id_component_material') IS NOT NULL
BEGIN
    UPDATE component
       SET component_name = COALESCE(
           NULLIF(LTRIM(RTRIM(material.description)), N''),
           NULLIF(LTRIM(RTRIM(material.insumo)), N''),
           CONCAT(N'Componente ', component.id_component_material))
      FROM dbo.materialcomponents component
      LEFT JOIN dbo.materials material ON material.id = component.id_component_material
     WHERE component.component_name IS NULL;

    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_materialcomponents_component')
        ALTER TABLE dbo.materialcomponents DROP CONSTRAINT FK_materialcomponents_component;
    IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_materialcomponents_not_self')
        ALTER TABLE dbo.materialcomponents DROP CONSTRAINT CK_materialcomponents_not_self;
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.materialcomponents') AND name = N'UX_materialcomponents_active_parent_component')
        DROP INDEX UX_materialcomponents_active_parent_component ON dbo.materialcomponents;
    ALTER TABLE dbo.materialcomponents DROP COLUMN id_component_material;
END;
GO

UPDATE dbo.materialcomponents
   SET component_name = CONCAT(N'Componente ', id)
 WHERE component_name IS NULL;
GO

ALTER TABLE dbo.materialcomponents ALTER COLUMN component_name NVARCHAR(200) NOT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.materialcomponents') AND name = N'IX_materialcomponents_active_parent_component')
    CREATE INDEX IX_materialcomponents_active_parent_component
        ON dbo.materialcomponents(id_company, id_parent_material, component_name)
        WHERE active = 1;
GO

IF COL_LENGTH(N'dbo.Detailsinandout', N'component_name') IS NULL
    ALTER TABLE dbo.Detailsinandout ADD component_name NVARCHAR(200) NULL;
IF COL_LENGTH(N'dbo.Detailsinandout', N'source_quantity') IS NULL
    ALTER TABLE dbo.Detailsinandout ADD source_quantity DECIMAL(15,4) NULL;
IF COL_LENGTH(N'dbo.Detailsinandout', N'source_measure') IS NULL
    ALTER TABLE dbo.Detailsinandout ADD source_measure NVARCHAR(30) NULL;
IF COL_LENGTH(N'dbo.Detailsinandout', N'grams_per_meter_at_movement') IS NULL
    ALTER TABLE dbo.Detailsinandout ADD grams_per_meter_at_movement DECIMAL(15,4) NULL;
GO

ALTER TABLE dbo.Detailsinandout ALTER COLUMN id_product INT NULL;
GO

IF COL_LENGTH(N'dbo.materials', N'grams_per_meter') IS NULL
    ALTER TABLE dbo.materials ADD grams_per_meter DECIMAL(15,4) NULL;
GO
