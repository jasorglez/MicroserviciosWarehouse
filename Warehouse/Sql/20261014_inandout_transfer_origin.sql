/* Adds source and paired-movement references for warehouse transfers.
   id_warehouse remains the warehouse that owns each movement. */
IF COL_LENGTH('dbo.inandout', 'id_warehouse_origin') IS NULL
    ALTER TABLE dbo.inandout ADD id_warehouse_origin INT NULL;
GO

IF COL_LENGTH('dbo.inandout', 'id_transfer_pair') IS NULL
    ALTER TABLE dbo.inandout ADD id_transfer_pair INT NULL;
GO

IF COL_LENGTH('dbo.Detailsinandout', 'id_transfer_pair') IS NULL
    ALTER TABLE dbo.Detailsinandout ADD id_transfer_pair INT NULL;
GO
