use warehouses;
/* Ejecutar en la base de datos warehouses. Este archivo se mantiene local y no se sube a Git. */
IF OBJECT_ID('dbo.inandoutevidence', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.inandoutevidence (
        id           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_inandoutevidence PRIMARY KEY,
        id_inandout  INT NOT NULL,
        image        VARCHAR(255) NOT NULL,
        comment      NVARCHAR(200) NULL,
        created_at   DATETIME NOT NULL CONSTRAINT DF_inandoutevidence_created_at DEFAULT GETUTCDATE(),
        active       BIT NOT NULL CONSTRAINT DF_inandoutevidence_active DEFAULT 1,
        CONSTRAINT FK_inandoutevidence_inandout FOREIGN KEY (id_inandout) REFERENCES dbo.inandout(id)
    );
    CREATE INDEX IX_inandoutevidence_id_inandout ON dbo.inandoutevidence(id_inandout);
END;
