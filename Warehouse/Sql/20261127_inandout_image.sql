use warehouses;
/* Ejecutar en la base de datos warehouses. Este archivo se mantiene local y no se sube a Git. */
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.inandout') AND name = 'image'
)
BEGIN
    ALTER TABLE dbo.inandout ADD image VARCHAR(255) NULL;
END;
