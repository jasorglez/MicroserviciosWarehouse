SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- Alcance limitado: solo proyectos activos asignados a la empresa 10.
;WITH company10_projects AS
(
    SELECT DISTINCT p.id
    FROM smp.dbo.projects p
    JOIN smp.dbo.contracts c ON c.id = p.id_contrato
    JOIN smp.dbo.branchs b ON b.id = c.id_branch
    WHERE b.id_company = 10 AND p.active = 1
), active_rows AS
(
    SELECT
        ps.id,
        ps.type,
        ps.id_project_or_branch,
        MIN(ps.id) OVER (PARTITION BY ps.type, ps.id_project_or_branch) AS keeper_id,
        MAX(ISNULL(ps.consecutive_entry, 0)) OVER (PARTITION BY ps.type, ps.id_project_or_branch) AS max_entry,
        MAX(ISNULL(ps.consecutive_out, 0)) OVER (PARTITION BY ps.type, ps.id_project_or_branch) AS max_out
    FROM dbo.prefix_setup ps
    JOIN company10_projects p ON p.id = ps.id_project_or_branch
    WHERE ps.type = 'project' AND ps.active = 1
), folio_maxima AS
(
    SELECT
        p.id AS project_id,
        MAX(CASE WHEN UPPER(m.type) = 'IN'
            THEN TRY_CONVERT(int, RIGHT(LTRIM(RTRIM(m.folio)), PATINDEX('%[^0-9]%', REVERSE(LTRIM(RTRIM(m.folio))) + 'X') - 1)) END) AS used_entry,
        MAX(CASE WHEN UPPER(m.type) = 'OUT'
            THEN TRY_CONVERT(int, RIGHT(LTRIM(RTRIM(m.folio)), PATINDEX('%[^0-9]%', REVERSE(LTRIM(RTRIM(m.folio))) + 'X') - 1)) END) AS used_out
    FROM company10_projects p
    LEFT JOIN dbo.inandout m ON m.id_project = p.id
    GROUP BY p.id
)
UPDATE keeper
SET consecutive_entry = CASE WHEN fm.used_entry > ar.max_entry THEN fm.used_entry ELSE ar.max_entry END,
    consecutive_out = CASE WHEN fm.used_out > ar.max_out THEN fm.used_out ELSE ar.max_out END
FROM dbo.prefix_setup keeper
JOIN active_rows ar ON ar.keeper_id = keeper.id AND ar.id = keeper.id
LEFT JOIN folio_maxima fm ON fm.project_id = ar.id_project_or_branch;

-- Conserva la fila activa más antigua por proyecto y desactiva solo duplicados de empresa 10.
;WITH company10_projects AS
(
    SELECT DISTINCT p.id
    FROM smp.dbo.projects p
    JOIN smp.dbo.contracts c ON c.id = p.id_contrato
    JOIN smp.dbo.branchs b ON b.id = c.id_branch
    WHERE b.id_company = 10 AND p.active = 1
), ranked AS
(
    SELECT ps.id, ROW_NUMBER() OVER (PARTITION BY ps.type, ps.id_project_or_branch ORDER BY ps.id) AS row_num
    FROM dbo.prefix_setup ps
    JOIN company10_projects p ON p.id = ps.id_project_or_branch
    WHERE ps.type = 'project' AND ps.active = 1
)
UPDATE ps SET active = 0
FROM dbo.prefix_setup ps
JOIN ranked r ON r.id = ps.id
WHERE r.row_num > 1;

-- Completa prefijos predeterminados en los proyectos activos de empresa 10 sin configuración.
;WITH company10_projects AS
(
    SELECT DISTINCT p.id
    FROM smp.dbo.projects p
    JOIN smp.dbo.contracts c ON c.id = p.id_contrato
    JOIN smp.dbo.branchs b ON b.id = c.id_branch
    WHERE b.id_company = 10 AND p.active = 1
)
INSERT INTO dbo.prefix_setup
    (id_project_or_branch, type, prefix_req, consecutive_req, prefix_oc, consecutive_oc,
     prefix_entry, consecutive_entry, prefix_out, consecutive_out, active)
SELECT p.id, 'project', 'RQ-', 0, 'OC-', 0, 'EN-', 0, 'SA-', 0, 1
FROM company10_projects p
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.prefix_setup ps
    WHERE ps.id_project_or_branch = p.id AND ps.type = 'project' AND ps.active = 1
);

COMMIT TRANSACTION;

SELECT p.id AS project_id, p.name AS project_name,
       ps.id AS setup_id, ps.prefix_entry, ps.consecutive_entry,
       ps.prefix_out, ps.consecutive_out, ps.active AS setup_active
FROM smp.dbo.projects p
JOIN smp.dbo.contracts c ON c.id = p.id_contrato
JOIN smp.dbo.branchs b ON b.id = c.id_branch
LEFT JOIN dbo.prefix_setup ps ON ps.id_project_or_branch = p.id AND ps.type = 'project' AND ps.active = 1
WHERE b.id_company = 10 AND p.active = 1
ORDER BY p.id;
GO
