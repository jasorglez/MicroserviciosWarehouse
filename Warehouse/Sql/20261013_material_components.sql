USE warehouses;
GO

/* Componentes por material. Ejecutar una sola vez en la base warehouses. */
IF OBJECT_ID(N'dbo.materialcomponents', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.materialcomponents
    (
        id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_materialcomponents PRIMARY KEY,
        id_company INT NOT NULL,
        id_parent_material INT NOT NULL,
        component_name NVARCHAR(200) NOT NULL,
        quantity DECIMAL(15,4) NOT NULL,
        active BIT NOT NULL CONSTRAINT DF_materialcomponents_active DEFAULT (1),
        CONSTRAINT CK_materialcomponents_quantity CHECK (quantity > 0),
        CONSTRAINT FK_materialcomponents_parent FOREIGN KEY (id_parent_material) REFERENCES dbo.materials(id)
    );

    CREATE INDEX IX_materialcomponents_active_parent_component
        ON dbo.materialcomponents(id_company, id_parent_material, component_name)
        WHERE active = 1;
END;
GO

/* La línea de inventario apunta al componente; esta columna conserva el padre
   recibido para rastrear el desglose original. */
IF COL_LENGTH(N'dbo.Detailsinandout', N'id_parent_product') IS NULL
BEGIN
    ALTER TABLE dbo.Detailsinandout ADD id_parent_product INT NULL;
    ALTER TABLE dbo.Detailsinandout ADD CONSTRAINT FK_Detailsinandout_parent_material
        FOREIGN KEY (id_parent_product) REFERENCES dbo.materials(id);
END;
GO

IF COL_LENGTH(N'dbo.Detailsinandout', N'component_name') IS NULL
    ALTER TABLE dbo.Detailsinandout ADD component_name NVARCHAR(200) NULL;
GO

IF COL_LENGTH(N'dbo.Detailsinandout', N'source_quantity') IS NULL
    ALTER TABLE dbo.Detailsinandout ADD source_quantity DECIMAL(15,4) NULL;
IF COL_LENGTH(N'dbo.Detailsinandout', N'source_measure') IS NULL
    ALTER TABLE dbo.Detailsinandout ADD source_measure NVARCHAR(30) NULL;
IF COL_LENGTH(N'dbo.Detailsinandout', N'grams_per_meter_at_movement') IS NULL
    ALTER TABLE dbo.Detailsinandout ADD grams_per_meter_at_movement DECIMAL(15,4) NULL;
GO

IF COL_LENGTH(N'dbo.materials', N'grams_per_meter') IS NULL
    ALTER TABLE dbo.materials ADD grams_per_meter DECIMAL(15,4) NULL;
GO

ALTER TABLE dbo.Detailsinandout ALTER COLUMN id_product INT NULL;
GO
