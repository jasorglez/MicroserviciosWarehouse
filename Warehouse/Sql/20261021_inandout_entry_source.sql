use warehouses;
/* Links each warehouse outing to the inbound voucher whose balance it consumes. */
IF COL_LENGTH(N'dbo.inandout', N'id_entry') IS NULL
    ALTER TABLE dbo.inandout ADD id_entry INT NULL;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.inandout')
      AND name = N'IX_inandout_id_entry_active'
)
    CREATE INDEX IX_inandout_id_entry_active ON dbo.inandout(id_entry, active, type);
GO
