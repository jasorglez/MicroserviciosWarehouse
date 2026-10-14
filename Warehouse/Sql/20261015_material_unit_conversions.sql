/* Conversiones configurables entre unidades por material.
   Regla: 1 unidad origen equivale a factor unidades destino. */
IF OBJECT_ID(N'dbo.materialconversions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.materialconversions
    (
        id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_materialconversions PRIMARY KEY,
        id_company INT NOT NULL,
        id_material INT NOT NULL,
        id_measure_from INT NOT NULL,
        id_measure_to INT NOT NULL,
        factor DECIMAL(18,6) NOT NULL,
        active BIT NOT NULL CONSTRAINT DF_materialconversions_active DEFAULT (1),
        CONSTRAINT CK_materialconversions_factor CHECK (factor > 0),
        CONSTRAINT CK_materialconversions_measures CHECK (id_measure_from <> id_measure_to)
    );
END;

IF COL_LENGTH(N'dbo.Detailsinandout', N'conversion_factor_at_movement') IS NULL
    ALTER TABLE dbo.Detailsinandout ADD conversion_factor_at_movement DECIMAL(18,6) NULL;

IF COL_LENGTH(N'dbo.Detailsinandout', N'target_measure_at_movement') IS NULL
    ALTER TABLE dbo.Detailsinandout ADD target_measure_at_movement NVARCHAR(30) NULL;

IF COL_LENGTH(N'dbo.Detailsinandout', N'source_measure_id') IS NULL
    ALTER TABLE dbo.Detailsinandout ADD source_measure_id INT NULL;

IF COL_LENGTH(N'dbo.Detailsinandout', N'target_measure_id_at_movement') IS NULL
    ALTER TABLE dbo.Detailsinandout ADD target_measure_id_at_movement INT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.materialconversions') AND name = N'UX_materialconversions_active_direction')
    CREATE UNIQUE INDEX UX_materialconversions_active_direction
        ON dbo.materialconversions(id_company, id_material, id_measure_from)
        WHERE active = 1;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.materialconversions') AND name = N'IX_materialconversions_material')
    CREATE INDEX IX_materialconversions_material
        ON dbo.materialconversions(id_company, id_material, active);

/* Migra la parametrización anterior gramos/metro a kg -> metro cuando puede
   identificar ambas unidades en el catálogo de la empresa. */
INSERT INTO dbo.materialconversions
    (id_company, id_material, id_measure_from, id_measure_to, factor, active)
SELECT m.id_company, m.id, source_measure.id, target_measure.id,
       CONVERT(DECIMAL(18,6), 1000.0 / NULLIF(m.grams_per_meter, 0)), 1
FROM dbo.materials m
JOIN dbo.catalog source_measure ON source_measure.id = m.id_medida
CROSS APPLY
(
    SELECT TOP (1) c.id
    FROM dbo.catalog c
    WHERE c.id_company = m.id_company AND c.type = 'MEASURE' AND c.active = 1
      AND UPPER(LTRIM(RTRIM(c.description))) IN ('M', 'METRO', 'METROS')
    ORDER BY CASE UPPER(LTRIM(RTRIM(c.description))) WHEN 'M' THEN 0 WHEN 'METRO' THEN 1 ELSE 2 END, c.id
) target_measure
WHERE m.grams_per_meter > 0
  AND UPPER(LTRIM(RTRIM(source_measure.description))) IN ('KG', 'KGS', 'KILO', 'KILOS', 'KILOGRAMO', 'KILOGRAMOS')
  AND NOT EXISTS
  (
      SELECT 1 FROM dbo.materialconversions existing
      WHERE existing.id_company = m.id_company AND existing.id_material = m.id
        AND existing.id_measure_from = source_measure.id AND existing.id_measure_to = target_measure.id AND existing.active = 1
  );
