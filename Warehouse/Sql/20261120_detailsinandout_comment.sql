IF COL_LENGTH(N'dbo.Detailsinandout', N'comment') IS NULL
BEGIN
    ALTER TABLE dbo.Detailsinandout ADD comment NVARCHAR(500) NULL;
END;
