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
        id_component_material INT NOT NULL,
        quantity DECIMAL(15,4) NOT NULL,
        active BIT NOT NULL CONSTRAINT DF_materialcomponents_active DEFAULT (1),
        CONSTRAINT CK_materialcomponents_quantity CHECK (quantity > 0),
        CONSTRAINT CK_materialcomponents_not_self CHECK (id_parent_material <> id_component_material),
        CONSTRAINT FK_materialcomponents_parent FOREIGN KEY (id_parent_material) REFERENCES dbo.materials(id),
        CONSTRAINT FK_materialcomponents_component FOREIGN KEY (id_component_material) REFERENCES dbo.materials(id)
    );

    CREATE UNIQUE INDEX UX_materialcomponents_active_parent_component
        ON dbo.materialcomponents(id_company, id_parent_material, id_component_material)
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
