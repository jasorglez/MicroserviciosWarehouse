using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Warehouse.Models;
using Warehouse.Models.DTOs;
using Warehouse.Models.Delison;

namespace Warehouse.Service.Delison
{
    public interface IGastosService
    {
        Task<ExpenseReportDto> GetExpenseReport(int idCompany, DateTime startDate, DateTime endDate, string lens);
        Task<List<PendingPaymentDto>> GetPendingPayments(int idCompany);
        Task<List<PendingPaymentDto>> GetPaidPayments(int idCompany);
        Task<bool> ConfirmPayment(ConfirmPaymentDto dto);
        Task<bool> SavePending(ConfirmPaymentDto dto);
        Task<bool> ActivarCredito(ActivarCreditoDto dto);
        Task<bool> MarcarAnticipo(MarcarAnticipoDto dto);
        Task<bool> ConfirmAnticipo(ConfirmAnticipoDto dto);
    }

    /// <summary>
    /// Reporte gerencial de gastos. Agrega el gasto del proyecto cruzando varias bases de datos
    /// (warehouses + smp.dbo.Branchs + security.dbo.Roles) en UN SOLO query con GROUP BY en SQL.
    /// Todas las BD viven en la misma instancia, por eso se usa raw SQL con nombres de 3 partes.
    /// </summary>
    public class GastosService : IGastosService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<GastosService> _logger;

        public GastosService(DbWarehouseContext context, ILogger<GastosService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ExpenseReportDto> GetExpenseReport(int idCompany, DateTime startDate, DateTime endDate, string lens)
        {
            var normalizedLens = (lens ?? "PAGADO").Trim().ToUpperInvariant();
            if (normalizedLens != "COMPROMETIDO") normalizedLens = "PAGADO";

            // Normalizar el rango: inicio del día de start, fin del día de end.
            var start = startDate.Date;
            var end = endDate.Date.AddDays(1).AddTicks(-1); // 23:59:59.9999999

            var result = new ExpenseReportDto
            {
                Lens = normalizedLens,
                StartDate = start,
                EndDate = endDate.Date
            };

            // ── Lente PAGADO (cash out real): suma entradas_molienda.pago de entradas LIBERADAS (liberacion=1), ancladas por fecha_pago ──
            // ── Lente COMPROMETIDO: suma detailsreqoc.price*quantity de OCs por datecreate     ──
            string sql = normalizedLens == "PAGADO"
                ? @"
                    SELECT idBranch, branchName, idDepartament, departmentName,
                           SUM(total) AS total, SUM(numTransacciones) AS numTransacciones
                    FROM (
                    SELECT
                        (CASE WHEN o.[type] = 'CR' THEN oreq.id_reference ELSE o.id_reference END) AS idBranch,
                        b.name                                                    AS branchName,
                        o.id_departament                                          AS idDepartament,
                        r.[Description]                                           AS departmentName,
                        SUM(CAST(ISNULL(e.monto_mxn, ISNULL(e.pago, 0)) AS DECIMAL(18,2))) AS total,  -- Fase 4: total en MXN
                        COUNT(*)                                                  AS numTransacciones
                    FROM warehouses.Delison.entradas_molienda e
                    INNER JOIN warehouses.dbo.ocandreq o ON o.id = e.id_oc
                    -- En CR el branch está en la REQUIS padre (o.id_req), no en o.id_reference (ahí va el id del item origen).
                    LEFT  JOIN warehouses.dbo.ocandreq oreq ON oreq.id = o.id_req
                    INNER JOIN smp.dbo.Branchs b ON b.id = (CASE WHEN o.[type] = 'CR' THEN oreq.id_reference ELSE o.id_reference END)
                    LEFT  JOIN security.dbo.Roles r ON r.id = o.id_departament
                -- Iniciales del proveedor en el folio CR (consecutive_oc_proveedor por sucursal)
                LEFT  JOIN warehouses.dbo.prefix_setup ps ON ps.id_project_or_branch = (CASE WHEN o.[type] = 'CR' THEN oreq.id_reference ELSE o.id_reference END) AND ps.[type] = 'branch' AND ps.active = 1
                    WHERE e.active = 1
                      AND e.liberacion = 1            -- solo entradas YA pagadas/liberadas en Gastos
                      AND b.id_company = @idCompany
                      AND e.fecha_pago >= @start       -- ancla = fecha en que se confirmó el pago
                      AND e.fecha_pago <= @end
                    GROUP BY (CASE WHEN o.[type] = 'CR' THEN oreq.id_reference ELSE o.id_reference END), b.name, o.id_departament, r.[Description]
                    UNION ALL
                    -- Gastos generales PAGADOS (anticipos, etc.) anclados por su fecha_pago.
                    SELECT
                        g.id_branch       AS idBranch,
                        b2.name           AS branchName,
                        g.id_departament  AS idDepartament,
                        r2.[Description]  AS departmentName,
                        SUM(CAST(ISNULL(g.monto_mxn, ISNULL(g.monto, 0)) AS DECIMAL(18,2))) AS total,  -- Fase 4: total en MXN
                        COUNT(*)          AS numTransacciones
                    FROM warehouses.Delison.gastos_generales g
                    INNER JOIN smp.dbo.Branchs b2 ON b2.id = g.id_branch
                    LEFT  JOIN security.dbo.Roles r2 ON r2.id = g.id_departament
                    WHERE g.active = 1 AND g.estado = 'PAGADO' AND b2.id_company = @idCompany
                      AND g.fecha_pago >= @start AND g.fecha_pago <= @end
                    GROUP BY g.id_branch, b2.name, g.id_departament, r2.[Description]
                    ) t
                    GROUP BY idBranch, branchName, idDepartament, departmentName"
                : @"
                    SELECT
                        o.id_reference                                            AS idBranch,
                        b.name                                                    AS branchName,
                        o.id_departament                                          AS idDepartament,
                        r.[Description]                                           AS departmentName,
                        SUM(CAST(ISNULL(d.quantity, 0) AS DECIMAL(18,2)) * CAST(ISNULL(d.price, 0) AS DECIMAL(18,2))) AS total,
                        COUNT(DISTINCT o.id)                                      AS numTransacciones
                    FROM warehouses.dbo.ocandreq o
                    INNER JOIN warehouses.dbo.detailsreqoc d ON d.id_movement = o.id AND d.active = 1
                    INNER JOIN smp.dbo.Branchs b ON b.id = o.id_reference
                    LEFT  JOIN security.dbo.Roles r ON r.id = o.id_departament
                -- Iniciales del proveedor en el folio CR (consecutive_oc_proveedor por sucursal)
                LEFT  JOIN warehouses.dbo.prefix_setup ps ON ps.id_project_or_branch = (CASE WHEN o.[type] = 'CR' THEN oreq.id_reference ELSE o.id_reference END) AND ps.[type] = 'branch' AND ps.active = 1
                    WHERE o.active = 1
                      AND o.[type] = 'OC'
                      AND b.id_company = @idCompany
                      AND o.datecreate >= @start
                      AND o.datecreate <= @end
                    GROUP BY o.id_reference, b.name, o.id_departament, r.[Description]";

            try
            {
                var connection = _context.Database.GetDbConnection();
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = sql;

                AddParam(cmd, "@idCompany", idCompany);
                AddParam(cmd, "@start", start);
                AddParam(cmd, "@end", end);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var cell = new ExpenseReportCellDto
                    {
                        IdBranch         = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0)),
                        BranchName       = reader.IsDBNull(1) ? "Sin Sucursal" : reader.GetString(1),
                        IdDepartament    = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader.GetValue(2)),
                        DepartmentName   = reader.IsDBNull(3) ? "Sin Departamento" : reader.GetString(3),
                        Total            = reader.IsDBNull(4) ? 0m : Convert.ToDecimal(reader.GetValue(4)),
                        NumTransacciones = reader.IsDBNull(5) ? 0 : Convert.ToInt32(reader.GetValue(5))
                    };
                    result.Cells.Add(cell);
                }

                result.GrandTotal         = result.Cells.Sum(c => c.Total);
                result.TotalTransacciones = result.Cells.Sum(c => c.NumTransacciones);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando reporte de gastos. Company={IdCompany}, Lens={Lens}", idCompany, normalizedLens);
                throw;
            }
        }

        // ── Captura de Gastos: entradas pendientes de pago (liberacion=0 y cierre de nivel activo) ──
        public async Task<List<PendingPaymentDto>> GetPendingPayments(int idCompany)
        {
            // Lógica unificada de "lista para Gastos":
            //  · multi-entrega  → entregas_oc.close = 1
            //  · sin límite     → entradas_molienda.close = 1
            //  · CR / 1 entrega → ocandreq.close = 1
            const string sql = @"
                SELECT
                    e.id                                         AS idEntrada,
                    e.id_oc                                      AS idOc,
                    CASE WHEN o.[type] = 'CR'
                         THEN 'CR-' + REPLACE(ISNULL(oreq.folio, ''), '-', '')
                              + CASE WHEN ISNULL(r.prefijo, '') <> '' THEN '-' + r.prefijo ELSE '' END
                              + CASE WHEN e.liberacion = 1 AND ISNULL(o.id_provider, 0) > 0
                                          AND ISNULL(NULLIF(d.name_provider, ''), d.provint) IS NOT NULL
                                     THEN '-' + UPPER(LEFT(LTRIM(ISNULL(NULLIF(d.name_provider, ''), d.provint)),
                                                           ISNULL(NULLIF(ps.consecutive_oc_proveedor, 0), 3)))
                                              + CAST(o.id_provider AS VARCHAR(20))
                                     ELSE '' END
                         ELSE COALESCE(NULLIF(e.folio_entrega, ''), o.folio) END AS folio,
                    o.[type]                                     AS docType,
                    d.typeoc                                     AS tipoOc,
                    CASE WHEN e.id_entrega IS NOT NULL THEN 'ENTREGA'
                         WHEN d.typeoc = 'COMPRA AUTORIZADA SIN LIMITE' THEN 'SIN_LIMITE'
                         ELSE 'OC' END                           AS closeSource,
                    (CASE WHEN o.[type] = 'CR' THEN oreq.id_reference ELSE o.id_reference END) AS idReference,
                    b.name                                       AS branchName,
                    o.id_departament                             AS idDepartament,
                    r.[Description]                              AS departmentName,
                    e.id_material                                AS idMaterial,
                    d.namearticle                                AS articulo,
                    d.numarticle                                 AS numArticulo,
                    d.id                                         AS idDetail,
                    e.id_entrega                                 AS idEntrega,
                    ISNULL(NULLIF(d.name_provider, ''), d.provint) AS proveedor,
                    ISNULL(e.cantidad_entrada, 0)                AS cantidad,
                    ISNULL(d.quantity, 0)                        AS cantidadOc,
                    ISNULL(dreq.quantity, 0)                     AS cantidadReq,
                    ISNULL(d.price, 0)                           AS precioUnitario,
                    ISNULL(e.pago, 0)                            AS valorPago,
                    cur.valueAddition                            AS monedaItem,   -- Fase 4: moneda del ítem (para convertir al pagar)
                    -- IVA por entrega SOLO en multi-entrega (>1 entrega del ítem): se lee de entregas_oc.
                    -- Single-entrega/CR/sin-límite (<=1) → del detalle del ítem (d.mas_iva). El valor no cambia.
                    ISNULL(CASE WHEN (SELECT COUNT(*) FROM warehouses.Delison.entregas_oc eg2
                                      WHERE eg2.id_detailsreqoc = d.id AND eg2.active = 1) > 1
                                THEN eg.mas_iva ELSE d.mas_iva END, 0) AS masIva,
                    COALESCE(NULLIF(e.nota_factura, ''), eg.nota_factura) AS notaFactura,
                    e.num_nota_factura                           AS numNotaFactura,
                    e.fecha_recepcion                            AS fechaRecepcion,
                    e.fecha_pago                                 AS fechaPago,
                    ISNULL(cp.calculo_anticipo, 0)               AS calculoAnticipo,
                    ISNULL(cp.cantidad, 0)                       AS condicionCantidad,
                    ISNULL(e.credito, 0)                         AS credito,
                    e.fecha_vencimiento                          AS fechaVencimiento,
                    ISNULL(o.anticipo_pagado, 0)                 AS anticipoPagado,
                    ISNULL(o.anticipo_monto, 0)                  AS anticipoMonto,
                    (ISNULL(o.anticipo_monto, 0) - ISNULL((
                        SELECT SUM(ISNULL(e3.anticipo_aplicado, 0))
                        FROM warehouses.Delison.entradas_molienda e3
                        WHERE e3.id_oc = o.id AND e3.active = 1), 0)) AS anticipoSaldo,
                    o.metodo_anticipo                            AS metodoAnticipo,
                    o.num_prorrateo                              AS numProrrateo,
                    ISNULL((SELECT COUNT(*) FROM warehouses.Delison.entregas_oc eg3
                            WHERE eg3.id_detailsreqoc = d.id AND eg3.active = 1), 0) AS numEntregasPlan,
                    ISNULL((SELECT COUNT(*) FROM warehouses.Delison.entradas_molienda e4
                            WHERE e4.id_oc = o.id AND e4.id_material = e.id_material AND e4.active = 1), 0) AS numEntradasAlmacen
                FROM warehouses.Delison.entradas_molienda e
                INNER JOIN warehouses.dbo.ocandreq o ON o.id = e.id_oc
                LEFT  JOIN warehouses.dbo.detailsreqoc d ON d.id_movement = o.id AND d.id_supplie = e.id_material AND d.active = 1
                LEFT  JOIN warehouses.dbo.catalog cur ON cur.id = d.id_currency   -- Fase 4: moneda del ítem
                -- Detalle de la REQUIS padre para obtener la cantidad requisitada
                LEFT  JOIN warehouses.dbo.detailsreqoc dreq ON dreq.id_movement = o.id_req AND dreq.id_supplie = e.id_material AND dreq.active = 1
                LEFT  JOIN warehouses.dbo.condiciones_pago cp ON cp.id = COALESCE(o.id_condicion_pago,
                        (SELECT TOP 1 cz.id_condicion_pago FROM warehouses.dbo.ocandreq cz
                         WHERE cz.[type] = 'COTIZ' AND cz.id_req = o.id_req AND cz.id_provider = o.id_provider
                           AND cz.active = 1 AND cz.id_condicion_pago IS NOT NULL ORDER BY cz.id))
                LEFT  JOIN warehouses.Delison.entregas_oc eg ON eg.id = e.id_entrega
                -- En CR, o.id_reference NO es el branch (ahí va el id del item origen): el branch
                -- está en la REQUIS padre (o.id_req). Para OC, o.id_reference SÍ es el branch.
                LEFT  JOIN warehouses.dbo.ocandreq oreq ON oreq.id = o.id_req
                INNER JOIN smp.dbo.Branchs b ON b.id = (CASE WHEN o.[type] = 'CR' THEN oreq.id_reference ELSE o.id_reference END)
                LEFT  JOIN security.dbo.Roles r ON r.id = o.id_departament
                -- Iniciales del proveedor en el folio CR (consecutive_oc_proveedor por sucursal)
                LEFT  JOIN warehouses.dbo.prefix_setup ps ON ps.id_project_or_branch = (CASE WHEN o.[type] = 'CR' THEN oreq.id_reference ELSE o.id_reference END) AND ps.[type] = 'branch' AND ps.active = 1
                WHERE e.active = 1
                  AND e.liberacion = 0
                  AND b.id_company = @idCompany
                  AND (
                        (e.id_entrega IS NOT NULL AND eg.[close] = 1)
                     OR (e.id_entrega IS NULL AND d.typeoc = 'COMPRA AUTORIZADA SIN LIMITE' AND e.[close] = 1)
                     OR (e.id_entrega IS NULL AND d.typeoc = 'COMPRA INMEDIATA' AND e.[close] = 1)
                     OR (e.id_entrega IS NULL AND o.[type] = 'CR'
                         AND EXISTS (SELECT 1 FROM warehouses.Delison.entregas_oc eg2 WHERE eg2.id_detailsreqoc = d.id AND eg2.active = 1 AND eg2.[close] = 1))
                     OR (e.id_entrega IS NULL AND o.[type] = 'OC' AND (d.typeoc IS NULL OR d.typeoc <> 'COMPRA AUTORIZADA SIN LIMITE' AND d.typeoc <> 'COMPRA INMEDIATA')
                         AND EXISTS (SELECT 1 FROM warehouses.Delison.entregas_oc eg2 WHERE eg2.id_detailsreqoc = d.id AND eg2.active = 1 AND eg2.[close] = 1))
                  )
                ORDER BY o.id_reference, e.fecha_recepcion DESC, e.id DESC";

            var list = new List<PendingPaymentDto>();
            try
            {
                var connection = _context.Database.GetDbConnection();
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = sql;
                AddParam(cmd, "@idCompany", idCompany);

                using (var reader = await cmd.ExecuteReaderAsync())
                while (await reader.ReadAsync())
                {
                    list.Add(new PendingPaymentDto
                    {
                        IdEntrada      = GetInt(reader, "idEntrada"),
                        IdOc           = GetInt(reader, "idOc"),
                        Folio          = GetStr(reader, "folio"),
                        DocType        = GetStr(reader, "docType"),
                        TipoOc         = GetStrOrNull(reader, "tipoOc"),
                        CloseSource    = GetStr(reader, "closeSource"),
                        IdReference    = GetInt(reader, "idReference"),
                        BranchName     = GetStr(reader, "branchName"),
                        IdDepartament  = GetInt(reader, "idDepartament"),
                        DepartmentName = GetStrOrNull(reader, "departmentName") ?? "Sin Departamento",
                        IdMaterial     = GetIntOrNull(reader, "idMaterial"),
                        Articulo       = GetStr(reader, "articulo"),
                        NumArticulo    = GetStrOrNull(reader, "numArticulo"),
                        IdDetail       = GetIntOrNull(reader, "idDetail"),
                        IdEntrega      = GetIntOrNull(reader, "idEntrega"),
                        Proveedor      = GetStrOrNull(reader, "proveedor"),
                        Cantidad       = GetDec(reader, "cantidad"),
                        CantidadOc     = GetDec(reader, "cantidadOc"),
                        CantidadReq    = GetDec(reader, "cantidadReq"),
                        PrecioUnitario = GetDec(reader, "precioUnitario"),
                        ValorPago      = GetDec(reader, "valorPago"),
                        Moneda         = GetStrOrNull(reader, "monedaItem"),   // Fase 4: moneda del ítem a convertir
                        MasIva         = GetBool(reader, "masIva"),
                        NotaFactura    = GetStrOrNull(reader, "notaFactura"),
                        NumNotaFactura = GetStrOrNull(reader, "numNotaFactura"),
                        FechaRecepcion = GetDateOrNull(reader, "fechaRecepcion"),
                        FechaPago      = GetDateOrNull(reader, "fechaPago"),
                        CalculoAnticipo   = GetBool(reader, "calculoAnticipo"),
                        CondicionCantidad = GetInt(reader, "condicionCantidad"),
                        Credito           = GetBool(reader, "credito"),
                        FechaVencimiento  = GetDateOrNull(reader, "fechaVencimiento"),
                        AnticipoPagado    = GetBool(reader, "anticipoPagado"),
                        AnticipoMonto     = GetDec(reader, "anticipoMonto"),
                        AnticipoSaldo     = GetDec(reader, "anticipoSaldo"),
                        MetodoAnticipo    = GetStrOrNull(reader, "metodoAnticipo"),
                        NumProrrateo        = GetIntOrNull(reader, "numProrrateo"),
                        NumEntregasPlan     = GetInt(reader, "numEntregasPlan"),
                        NumEntradasAlmacen  = GetInt(reader, "numEntradasAlmacen"),
                    });
                }

                // ── FASE 3: Gastos generales EN TRÁMITE (anticipos, etc.) — aparecen en la Captura ──
                // No vienen de entradas_molienda; son filas de Delison.gastos_generales. Se marcan con
                // DocType = tipo_gasto ('ANTICIPO') e IdGastoGeneral para que el pago se confirme por su
                // propio flujo. Cantidad/PrecioUnitario van en 0 (un anticipo es un monto único = ValorPago).
                const string sqlGastos = @"
                    SELECT
                        g.id              AS idGasto,
                        g.id_oc           AS idOc,
                        g.folio           AS folio,
                        g.tipo_gasto      AS tipoGasto,
                        g.id_branch       AS idReference,
                        b.name            AS branchName,
                        g.id_departament  AS idDepartament,
                        r.[Description]   AS departmentName,
                        g.id_provider     AS idProvider,
                        g.proveedor       AS proveedor,
                        g.concepto        AS concepto,
                        g.monto           AS monto,
                        (SELECT TOP 1 cur2.valueAddition
                           FROM warehouses.dbo.detailsreqoc d2
                           LEFT JOIN warehouses.dbo.catalog cur2 ON cur2.id = d2.id_currency
                           WHERE d2.id_movement = g.id_oc AND d2.active = 1 AND d2.id_currency IS NOT NULL) AS monedaItem,
                        ISNULL(g.mas_iva, 0) AS masIva,
                        g.nota_factura    AS notaFactura,
                        g.fecha_registro  AS fechaRegistro
                    FROM warehouses.Delison.gastos_generales g
                    INNER JOIN smp.dbo.Branchs b ON b.id = g.id_branch
                    LEFT  JOIN security.dbo.Roles r ON r.id = g.id_departament
                    WHERE g.active = 1 AND g.estado = 'EN_TRAMITE' AND g.id_company = @idCompany
                    ORDER BY g.id DESC";

                using var cmdG = connection.CreateCommand();
                cmdG.CommandText = sqlGastos;
                AddParam(cmdG, "@idCompany", idCompany);
                using (var rg = await cmdG.ExecuteReaderAsync())
                while (await rg.ReadAsync())
                {
                    list.Add(new PendingPaymentDto
                    {
                        IdGastoGeneral = GetInt(rg, "idGasto"),
                        IdEntrada      = 0,
                        IdOc           = GetInt(rg, "idOc"),
                        Folio          = GetStr(rg, "folio"),
                        DocType        = GetStr(rg, "tipoGasto"),     // 'ANTICIPO'
                        IdReference    = GetInt(rg, "idReference"),
                        BranchName     = GetStr(rg, "branchName"),
                        IdDepartament  = GetInt(rg, "idDepartament"),
                        DepartmentName = GetStrOrNull(rg, "departmentName") ?? "Sin Departamento",
                        Articulo       = GetStrOrNull(rg, "concepto") ?? "Anticipo",
                        Proveedor      = GetStrOrNull(rg, "proveedor"),
                        Cantidad       = 0,
                        PrecioUnitario = 0,
                        ValorPago      = GetDec(rg, "monto"),
                        Moneda         = GetStrOrNull(rg, "monedaItem"),   // Fase 4: moneda del anticipo (ítems de la OC)
                        MasIva         = GetBool(rg, "masIva"),
                        NotaFactura    = GetStrOrNull(rg, "notaFactura"),
                        FechaRecepcion = GetDateOrNull(rg, "fechaRegistro"),
                    });
                }

                await EnrichAnticipoItems(list);
                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo entradas pendientes de pago. Company={IdCompany}", idCompany);
                throw;
            }
        }

        // ── Histórico de Pagos: entradas YA pagadas/liberadas (liberacion=1) ──
        public async Task<List<PendingPaymentDto>> GetPaidPayments(int idCompany)
        {
            const string sql = @"
                SELECT
                    e.id                                         AS idEntrada,
                    e.id_oc                                      AS idOc,
                    CASE WHEN o.[type] = 'CR'
                         THEN 'CR-' + REPLACE(ISNULL(oreq.folio, ''), '-', '')
                              + CASE WHEN ISNULL(r.prefijo, '') <> '' THEN '-' + r.prefijo ELSE '' END
                              + CASE WHEN e.liberacion = 1 AND ISNULL(o.id_provider, 0) > 0
                                          AND ISNULL(NULLIF(d.name_provider, ''), d.provint) IS NOT NULL
                                     THEN '-' + UPPER(LEFT(LTRIM(ISNULL(NULLIF(d.name_provider, ''), d.provint)),
                                                           ISNULL(NULLIF(ps.consecutive_oc_proveedor, 0), 3)))
                                              + CAST(o.id_provider AS VARCHAR(20))
                                     ELSE '' END
                         ELSE COALESCE(NULLIF(e.folio_entrega, ''), o.folio) END AS folio,
                    o.[type]                                     AS docType,
                    d.typeoc                                     AS tipoOc,
                    CASE WHEN e.id_entrega IS NOT NULL THEN 'ENTREGA'
                         WHEN d.typeoc = 'COMPRA AUTORIZADA SIN LIMITE' THEN 'SIN_LIMITE'
                         ELSE 'OC' END                           AS closeSource,
                    (CASE WHEN o.[type] = 'CR' THEN oreq.id_reference ELSE o.id_reference END) AS idReference,
                    b.name                                       AS branchName,
                    o.id_departament                             AS idDepartament,
                    r.[Description]                              AS departmentName,
                    e.id_material                                AS idMaterial,
                    d.namearticle                                AS articulo,
                    d.numarticle                                 AS numArticulo,
                    d.id                                         AS idDetail,
                    e.id_entrega                                 AS idEntrega,
                    ISNULL(NULLIF(d.name_provider, ''), d.provint) AS proveedor,
                    ISNULL(e.cantidad_entrada, 0)                AS cantidad,
                    ISNULL(d.price, 0)                           AS precioUnitario,
                    ISNULL(e.monto_mxn, ISNULL(e.pago, 0))       AS valorPago,   -- Fase 4: histórico en MXN
                    e.tipo_cambio                                AS tipoCambio,
                    e.moneda                                     AS moneda,
                    e.fuente_tc                                  AS fuenteTc,
                    -- TC con que se pagó el anticipo de esta OC (para convertir el anticipo aplicado a MXN).
                    (SELECT TOP 1 ga.tipo_cambio FROM warehouses.Delison.gastos_generales ga
                       WHERE ga.id_oc = o.id AND ga.tipo_gasto = 'ANTICIPO' AND ga.active = 1) AS tcAnticipo,
                    ISNULL(e.anticipo_aplicado, 0)              AS anticipoAplicado,
                    -- IVA por entrega SOLO en multi-entrega (>1 entrega del ítem): se lee de entregas_oc.
                    -- Single-entrega/CR/sin-límite (<=1) → del detalle del ítem (d.mas_iva). El valor no cambia.
                    ISNULL(CASE WHEN (SELECT COUNT(*) FROM warehouses.Delison.entregas_oc eg2
                                      WHERE eg2.id_detailsreqoc = d.id AND eg2.active = 1) > 1
                                THEN eg.mas_iva ELSE d.mas_iva END, 0) AS masIva,
                    COALESCE(NULLIF(e.nota_factura, ''), eg.nota_factura) AS notaFactura,
                    e.num_nota_factura                           AS numNotaFactura,
                    e.fecha_recepcion                            AS fechaRecepcion,
                    e.fecha_pago                                 AS fechaPago,
                    ISNULL(cp.calculo_anticipo, 0)               AS calculoAnticipo,
                    ISNULL(cp.cantidad, 0)                       AS condicionCantidad,
                    ISNULL(e.credito, 0)                         AS credito,
                    ISNULL(o.anticipo_pagado, 0)                 AS anticipoPagado,
                    ISNULL(o.anticipo_monto, 0)                  AS anticipoMonto,
                    (ISNULL(o.anticipo_monto, 0) - ISNULL((
                        SELECT SUM(ISNULL(e3.anticipo_aplicado, 0))
                        FROM warehouses.Delison.entradas_molienda e3
                        WHERE e3.id_oc = o.id AND e3.active = 1), 0)) AS anticipoSaldo,
                    o.metodo_anticipo                            AS metodoAnticipo,
                    o.num_prorrateo                              AS numProrrateo,
                    ISNULL((SELECT COUNT(*) FROM warehouses.Delison.entregas_oc eg3
                            WHERE eg3.id_detailsreqoc = d.id AND eg3.active = 1), 0) AS numEntregasPlan
                FROM warehouses.Delison.entradas_molienda e
                INNER JOIN warehouses.dbo.ocandreq o ON o.id = e.id_oc
                LEFT  JOIN warehouses.dbo.detailsreqoc d ON d.id_movement = o.id AND d.id_supplie = e.id_material AND d.active = 1
                LEFT  JOIN warehouses.dbo.condiciones_pago cp ON cp.id = COALESCE(o.id_condicion_pago,
                        (SELECT TOP 1 cz.id_condicion_pago FROM warehouses.dbo.ocandreq cz
                         WHERE cz.[type] = 'COTIZ' AND cz.id_req = o.id_req AND cz.id_provider = o.id_provider
                           AND cz.active = 1 AND cz.id_condicion_pago IS NOT NULL ORDER BY cz.id))
                LEFT  JOIN warehouses.Delison.entregas_oc eg ON eg.id = e.id_entrega
                -- En CR, o.id_reference NO es el branch (ahí va el id del item origen): el branch
                -- está en la REQUIS padre (o.id_req). Para OC, o.id_reference SÍ es el branch.
                LEFT  JOIN warehouses.dbo.ocandreq oreq ON oreq.id = o.id_req
                INNER JOIN smp.dbo.Branchs b ON b.id = (CASE WHEN o.[type] = 'CR' THEN oreq.id_reference ELSE o.id_reference END)
                LEFT  JOIN security.dbo.Roles r ON r.id = o.id_departament
                -- Iniciales del proveedor en el folio CR (consecutive_oc_proveedor por sucursal)
                LEFT  JOIN warehouses.dbo.prefix_setup ps ON ps.id_project_or_branch = (CASE WHEN o.[type] = 'CR' THEN oreq.id_reference ELSE o.id_reference END) AND ps.[type] = 'branch' AND ps.active = 1
                WHERE e.active = 1
                  AND e.liberacion = 1
                  AND b.id_company = @idCompany
                ORDER BY e.fecha_pago DESC, e.id DESC";

            var list = new List<PendingPaymentDto>();
            try
            {
                var connection = _context.Database.GetDbConnection();
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = sql;
                AddParam(cmd, "@idCompany", idCompany);

                using (var reader = await cmd.ExecuteReaderAsync())
                while (await reader.ReadAsync())
                {
                    list.Add(new PendingPaymentDto
                    {
                        IdEntrada      = GetInt(reader, "idEntrada"),
                        IdOc           = GetInt(reader, "idOc"),
                        Folio          = GetStr(reader, "folio"),
                        DocType        = GetStr(reader, "docType"),
                        TipoOc         = GetStrOrNull(reader, "tipoOc"),
                        CloseSource    = GetStr(reader, "closeSource"),
                        IdReference    = GetInt(reader, "idReference"),
                        BranchName     = GetStr(reader, "branchName"),
                        IdDepartament  = GetInt(reader, "idDepartament"),
                        DepartmentName = GetStrOrNull(reader, "departmentName") ?? "Sin Departamento",
                        IdMaterial     = GetIntOrNull(reader, "idMaterial"),
                        Articulo       = GetStr(reader, "articulo"),
                        NumArticulo    = GetStrOrNull(reader, "numArticulo"),
                        IdDetail       = GetIntOrNull(reader, "idDetail"),
                        IdEntrega      = GetIntOrNull(reader, "idEntrega"),
                        Proveedor      = GetStrOrNull(reader, "proveedor"),
                        Cantidad       = GetDec(reader, "cantidad"),
                        PrecioUnitario = GetDec(reader, "precioUnitario"),
                        ValorPago      = GetDec(reader, "valorPago"),
                        AnticipoAplicado = GetDec(reader, "anticipoAplicado"),
                        TipoCambio     = GetDecOrNull(reader, "tipoCambio"),
                        Moneda         = GetStrOrNull(reader, "moneda"),
                        FuenteTc       = GetStrOrNull(reader, "fuenteTc"),
                        TcAnticipo     = GetDecOrNull(reader, "tcAnticipo"),
                        MasIva         = GetBool(reader, "masIva"),
                        NotaFactura    = GetStrOrNull(reader, "notaFactura"),
                        NumNotaFactura = GetStrOrNull(reader, "numNotaFactura"),
                        FechaRecepcion = GetDateOrNull(reader, "fechaRecepcion"),
                        FechaPago      = GetDateOrNull(reader, "fechaPago"),
                        CalculoAnticipo   = GetBool(reader, "calculoAnticipo"),
                        CondicionCantidad = GetInt(reader, "condicionCantidad"),
                        Credito           = GetBool(reader, "credito"),
                        AnticipoPagado    = GetBool(reader, "anticipoPagado"),
                        AnticipoMonto     = GetDec(reader, "anticipoMonto"),
                        AnticipoSaldo     = GetDec(reader, "anticipoSaldo"),
                        MetodoAnticipo    = GetStrOrNull(reader, "metodoAnticipo"),
                        NumProrrateo      = GetIntOrNull(reader, "numProrrateo"),
                        NumEntregasPlan   = GetInt(reader, "numEntregasPlan"),
                    });
                }

                // ── FASE 5: Gastos generales PAGADOS (anticipos, etc.) — cuentan en el reporte/hoja del día ──
                // Anclados por fecha_pago (la que importa para cuadrar). El frontend filtra por rango de fechas.
                const string sqlGastosPagados = @"
                    SELECT
                        g.id              AS idGasto,
                        g.id_oc           AS idOc,
                        g.folio           AS folio,
                        g.tipo_gasto      AS tipoGasto,
                        g.id_branch       AS idReference,
                        b.name            AS branchName,
                        g.id_departament  AS idDepartament,
                        r.[Description]   AS departmentName,
                        g.proveedor       AS proveedor,
                        g.concepto        AS concepto,
                        ISNULL(g.monto_mxn, g.monto) AS monto,   -- Fase 4: histórico en MXN
                        g.tipo_cambio     AS tipoCambio,
                        g.moneda          AS moneda,
                        g.fuente_tc       AS fuenteTc,
                        ISNULL(g.mas_iva, 0) AS masIva,
                        g.nota_factura    AS notaFactura,
                        g.fecha_pago      AS fechaPago
                    FROM warehouses.Delison.gastos_generales g
                    INNER JOIN smp.dbo.Branchs b ON b.id = g.id_branch
                    LEFT  JOIN security.dbo.Roles r ON r.id = g.id_departament
                    WHERE g.active = 1 AND g.estado = 'PAGADO' AND g.id_company = @idCompany
                    ORDER BY g.fecha_pago DESC, g.id DESC";

                using var cmdGp = connection.CreateCommand();
                cmdGp.CommandText = sqlGastosPagados;
                AddParam(cmdGp, "@idCompany", idCompany);
                using (var rgp = await cmdGp.ExecuteReaderAsync())
                while (await rgp.ReadAsync())
                {
                    list.Add(new PendingPaymentDto
                    {
                        IdGastoGeneral = GetInt(rgp, "idGasto"),
                        IdEntrada      = 0,
                        IdOc           = GetInt(rgp, "idOc"),
                        Folio          = GetStr(rgp, "folio"),
                        DocType        = GetStr(rgp, "tipoGasto"),     // 'ANTICIPO'
                        IdReference    = GetInt(rgp, "idReference"),
                        BranchName     = GetStr(rgp, "branchName"),
                        IdDepartament  = GetInt(rgp, "idDepartament"),
                        DepartmentName = GetStrOrNull(rgp, "departmentName") ?? "Sin Departamento",
                        Articulo       = GetStrOrNull(rgp, "concepto") ?? "Anticipo",
                        Proveedor      = GetStrOrNull(rgp, "proveedor"),
                        Cantidad       = 0,
                        PrecioUnitario = 0,
                        ValorPago      = GetDec(rgp, "monto"),
                        TipoCambio     = GetDecOrNull(rgp, "tipoCambio"),
                        Moneda         = GetStrOrNull(rgp, "moneda"),
                        FuenteTc       = GetStrOrNull(rgp, "fuenteTc"),
                        TcAnticipo     = GetDecOrNull(rgp, "tipoCambio"),   // la fila ES el anticipo → su propio TC
                        MasIva         = GetBool(rgp, "masIva"),
                        NotaFactura    = GetStrOrNull(rgp, "notaFactura"),
                        FechaPago      = GetDateOrNull(rgp, "fechaPago"),
                    });
                }

                await EnrichAnticipoItems(list);
                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo entradas pagadas. Company={IdCompany}", idCompany);
                throw;
            }
        }

        // Adjunta a cada fila de anticipo el desglose de artículos de su OC (para el tooltip).
        // El precio/total replica la fórmula del comparador: round2(price × IVA si mas_iva) × cantidad.
        private async Task EnrichAnticipoItems(List<PendingPaymentDto> list)
        {
            var anticipoRows = list.Where(x => x.IdGastoGeneral.HasValue && x.IdOc > 0).ToList();
            // Entregas con anticipo: les adjuntamos el ledger de consumo para su tooltip de "Valor".
            var entregaRows = list.Where(x => !x.IdGastoGeneral.HasValue && x.AnticipoMonto > 0m && x.IdOc > 0).ToList();
            if (anticipoRows.Count == 0 && entregaRows.Count == 0) return;

            var ocIds = anticipoRows.Select(x => x.IdOc).Distinct().ToList();
            // OCs para el consumo: las de anticipos + las de entregas (su anticipo puede estar en otra lista).
            var consumoOcIds = ocIds.Concat(entregaRows.Select(x => x.IdOc)).Distinct().ToList();
            var branchIds = anticipoRows.Select(x => x.IdReference).Where(b => b > 0).Distinct().ToList();

            var ivaByBranch = branchIds.Count > 0
                ? await _context.Setups.AsNoTracking()
                    .Where(s => branchIds.Contains(s.IdBranch) && s.Active)
                    .GroupBy(s => s.IdBranch)
                    .Select(g => new { Branch = g.Key, Iva = g.Max(x => x.Iva) })
                    .ToDictionaryAsync(x => x.Branch, x => x.Iva ?? 0m)
                : new Dictionary<int, decimal>();

            var items = await _context.Detailsreqoc.AsNoTracking()
                .Where(d => ocIds.Contains(d.IdMovement) && d.Active == true)
                .Select(d => new { d.IdMovement, d.IdSupplie, d.TypeOc, d.NameArticle, d.Quantity, d.Price, d.MasIva })
                .ToListAsync();

            // Consumo del anticipo: entregas (entradas_molienda) de la OC con anticipo aplicado.
            var consumo = await _context.EntradasMolienda.AsNoTracking()
                .Where(e => consumoOcIds.Contains(e.IdOc) && e.Active
                         && e.AnticipoAplicado != null && e.AnticipoAplicado > 0)
                .OrderBy(e => e.Id)
                .Select(e => new { e.IdOc, e.FolioEntrega, e.AnticipoAplicado })
                .ToListAsync();

            // Porcentaje ORIGINAL del anticipo (condiciones_pago.cantidad). Por OC: id_condicion_pago de la
            // OC, o el de la COTIZ hermana (mismo id_req+id_provider) si la OC no lo trae. NO se recalcula con IVA.
            var ocsCond = await _context.Ocandreqs.AsNoTracking()
                .Where(o => ocIds.Contains(o.Id))
                .Select(o => new { o.Id, o.IdCondicionPago, o.IdReq, o.IdProvider })
                .ToListAsync();
            var ocCond = new Dictionary<int, int?>();
            var condIds = new HashSet<int>();
            foreach (var o in ocsCond)
            {
                int? cond = (o.IdCondicionPago.HasValue && o.IdCondicionPago.Value > 0) ? o.IdCondicionPago : null;
                if (cond == null)
                {
                    cond = await _context.Ocandreqs.AsNoTracking()
                        .Where(c => c.Type == "COTIZ" && c.IdReq == o.IdReq && c.IdProvider == o.IdProvider
                                 && c.Active == true && c.IdCondicionPago != null)
                        .OrderBy(c => c.Id)
                        .Select(c => c.IdCondicionPago)
                        .FirstOrDefaultAsync();
                }
                ocCond[o.Id] = cond;
                if (cond.HasValue && cond.Value > 0) condIds.Add(cond.Value);
            }
            var pctByCond = condIds.Count > 0
                ? await _context.CondicionesPago.AsNoTracking()
                    .Where(cp => condIds.Contains(cp.Id))
                    .ToDictionaryAsync(cp => cp.Id, cp => cp.Cantidad)
                : new Dictionary<int, int>();

            // Cantidad de la REQUISICIÓN padre por (REQUIS, material) — para ítems SIN LIMITE (cantidad_OC = 0).
            var ocReqMap = ocsCond.Where(o => o.IdReq.HasValue).ToDictionary(o => o.Id, o => o.IdReq!.Value);
            var reqIds = ocReqMap.Values.Distinct().ToList();
            var reqQtyMap = reqIds.Count > 0
                ? (await _context.Detailsreqoc.AsNoTracking()
                    .Where(d => reqIds.Contains(d.IdMovement) && d.Active == true)
                    .GroupBy(d => new { d.IdMovement, d.IdSupplie })
                    .Select(g => new { g.Key.IdMovement, g.Key.IdSupplie, Qty = g.Sum(x => x.Quantity) })
                    .ToListAsync())
                    .ToDictionary(x => (x.IdMovement, x.IdSupplie), x => x.Qty)
                : new Dictionary<(int, int), decimal>();

            foreach (var row in anticipoRows)
            {
                row.AnticipoPorcentaje =
                    (ocCond.TryGetValue(row.IdOc, out var cid) && cid.HasValue
                     && pctByCond.TryGetValue(cid.Value, out var cant)) ? cant : 0m;

                var iva = ivaByBranch.TryGetValue(row.IdReference, out var iv) ? iv : 0m;
                var factor = 1m + iva / 100m;
                var reqIdRow = ocReqMap.TryGetValue(row.IdOc, out var rid) ? rid : 0;
                row.AnticipoItems = items
                    .Where(i => i.IdMovement == row.IdOc)
                    .Select(i =>
                    {
                        var pu = (i.MasIva == true) ? Math.Round(i.Price * factor, 2) : i.Price;
                        // SIN LIMITE: cantidad_OC = 0 → usar la cantidad de la REQUISICIÓN (mismo material).
                        var qty = string.Equals(i.TypeOc, "COMPRA AUTORIZADA SIN LIMITE", StringComparison.OrdinalIgnoreCase)
                            ? (reqIdRow > 0 && reqQtyMap.TryGetValue((reqIdRow, i.IdSupplie), out var rq) ? rq : 0m)
                            : i.Quantity;
                        return new AnticipoItemDto
                        {
                            Articulo       = i.NameArticle ?? "",
                            Cantidad       = qty,
                            PrecioUnitario = pu,
                            Total          = pu * qty
                        };
                    })
                    .ToList();

                row.AnticipoConsumo = consumo
                    .Where(c => c.IdOc == row.IdOc)
                    .Select(c => new AnticipoConsumoDto
                    {
                        FolioEntrega = c.FolioEntrega ?? "",
                        Descuento    = c.AnticipoAplicado ?? 0m
                    })
                    .ToList();
            }

            // Mismo ledger de consumo para las filas de ENTREGA (mismo IdOc). El frontend lo recorta
            // hasta la entrada actual (por folio) para mostrar "Total → desglose → restante".
            foreach (var row in entregaRows)
            {
                row.AnticipoConsumo = consumo
                    .Where(c => c.IdOc == row.IdOc)
                    .Select(c => new AnticipoConsumoDto
                    {
                        FolioEntrega = c.FolioEntrega ?? "",
                        Descuento    = c.AnticipoAplicado ?? 0m
                    })
                    .ToList();
            }
        }

        // ── Captura de Gastos: confirmar pago de una entrada (transaccional) ──
        public async Task<bool> ConfirmPayment(ConfirmPaymentDto dto)
        {
            if (dto == null || dto.IdEntrada <= 0) return false;

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var entrada = await _context.EntradasMolienda.FindAsync(dto.IdEntrada);
                if (entrada == null) { await tx.RollbackAsync(); return false; }

                // Idempotente: si ya estaba liberada, no re-aplicar (evita doble acumulación en sin límite).
                if (entrada.Liberacion) { await tx.RollbackAsync(); return true; }

                // 1) Entrada: pago + fecha + nota + liberación
                entrada.Pago        = dto.ValorPago;
                entrada.FechaPago   = dto.FechaPago.HasValue
                    ? DateOnly.FromDateTime(dto.FechaPago.Value)
                    : DateOnly.FromDateTime(DateTime.Now);
                entrada.NotaFactura    = dto.NotaFactura;
                entrada.NumNotaFactura = dto.NumNotaFactura;
                entrada.Liberacion  = true;
                entrada.DateModified = DateTime.UtcNow;

                // Fase 4: conversión a MXN del pago (moneda extranjera). MXN → TC=1.
                var tc = dto.TipoCambio.HasValue && dto.TipoCambio.Value > 0 ? dto.TipoCambio.Value : 1m;
                entrada.Moneda     = string.IsNullOrWhiteSpace(dto.Moneda) ? "MXN" : dto.Moneda.Trim().ToUpperInvariant();
                entrada.TipoCambio = tc;
                entrada.FuenteTc   = dto.FuenteTc;
                entrada.MontoMxn   = Math.Round(dto.ValorPago * tc, 2);

                // ¿Multi-entrega? (el ítem OC tiene más de una entrega). En ese caso el IVA es POR ENTREGA
                // (entregas_oc.mas_iva), no del ítem OC, para que una entrega pueda llevar IVA y otra no.
                bool esMultiEntrega = false;
                if (dto.IdEntrega.HasValue && dto.IdDetail.HasValue)
                {
                    var nEntregas = await _context.EntregasOc
                        .CountAsync(e => e.IdDetailsreqoc == dto.IdDetail.Value && e.Active);
                    esMultiEntrega = nEntregas > 1;
                }

                // 2) Writeback al detalle (detailsreqoc)
                if (dto.IdDetail.HasValue)
                {
                    var d = await _context.Detailsreqoc.FindAsync(dto.IdDetail.Value);
                    if (d != null)
                    {
                        // El IVA del ítem OC solo se fija en single-entrega/CR/sin-límite. En multi-entrega
                        // NO se toca el detalle compartido (el IVA va por entrega más abajo).
                        if (!esMultiEntrega) d.MasIva = dto.MasIva;
                        if (string.Equals(dto.DocType, "CR", StringComparison.OrdinalIgnoreCase))
                        {
                            // Compra rápida: rellenar proveedor, precio unitario y moneda.
                            if (!string.IsNullOrWhiteSpace(dto.Proveedor)) d.NameProvider = dto.Proveedor;
                            if (dto.PrecioUnitario.HasValue) d.Price = dto.PrecioUnitario.Value;
                            if (dto.IdCurrency.HasValue && dto.IdCurrency.Value > 0) d.IdCurrency = dto.IdCurrency.Value;
                            // Id del proveedor: en el detalle y en el documento CR, para componer el
                            // folio CR-{sucursal}-{depto}-{abrev}{id} en todas las vistas de nomenclatura.
                            if (dto.IdProvider.HasValue && dto.IdProvider.Value > 0)
                            {
                                d.IdProvider = dto.IdProvider.Value;
                                var crOc = await _context.Ocandreqs.FindAsync(entrada.IdOc);
                                if (crOc != null) crOc.IdProvider = dto.IdProvider.Value;
                            }
                        }
                        if (string.Equals(dto.CloseSource, "SIN_LIMITE", StringComparison.OrdinalIgnoreCase))
                        {
                            // Sin límite: acumular la cantidad de esta entrada en el detalle (Total se recalcula en SQL)
                            d.Quantity += dto.Cantidad;
                        }
                    }
                }

                // 3) Entrega (multi-entrega): nota/factura + IVA propio de la entrega.
                if (dto.IdEntrega.HasValue)
                {
                    var eg = await _context.EntregasOc.FindAsync(dto.IdEntrega.Value);
                    if (eg != null)
                    {
                        if (!string.IsNullOrEmpty(dto.NotaFactura)) eg.NotaFactura = dto.NotaFactura;
                        if (esMultiEntrega) eg.MasIva = dto.MasIva;   // IVA independiente de ESTA entrega
                    }
                }

                // 4) NO se propaga al comparador (slot COTIZ). El comparador es una FOTO del momento de
                // la comparación y no está diseñado para multi-entrega (mostraría toda la cantidad con un
                // IVA único, generando discrepancias). El IVA vive por entrega (almacén) y en la OC.

                // 5) Anticipo: registrar el monto aplicado a esta entrada y fijar método/N en la OC (1ª vez).
                if (dto.AnticipoAplicado.HasValue && dto.AnticipoAplicado.Value > 0)
                {
                    entrada.AnticipoAplicado = dto.AnticipoAplicado.Value;
                    var oc = await _context.Ocandreqs.FindAsync(entrada.IdOc);
                    if (oc != null && string.IsNullOrEmpty(oc.MetodoAnticipo) && !string.IsNullOrEmpty(dto.MetodoAnticipo))
                    {
                        oc.MetodoAnticipo = dto.MetodoAnticipo;   // se fija en la 1ª aplicación, queda igual para las demás
                        oc.NumProrrateo   = dto.NumProrrateo;
                    }
                }

                // 6) Almacén GLOBAL de materia prima: sumar la cantidad recibida al inventario
                // (sucursal + departamento + material). Es el "almacén final" que usará producción.
                // Si la entrada ingresó a CRÉDITO, ya se sumó al activar el crédito → no duplicar.
                if (!entrada.Credito)
                    await AcumularInventarioMp(entrada);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Error confirmando pago de la entrada {IdEntrada}", dto.IdEntrada);
                throw;
            }
        }

        // Suma la cantidad de una entrada LIBERADA al almacén global de materia prima (inventario_mp),
        // agrupado por (empresa, sucursal, departamento, material). Upsert (find-or-create). Corre dentro
        // de la transacción del llamador (se persiste con su SaveChangesAsync).
        private async Task AcumularInventarioMp(EntradaMoliendaDelison entrada)
        {
            if (entrada == null || !entrada.IdMaterial.HasValue || entrada.IdMaterial.Value <= 0) return;
            var qty = entrada.CantidadEntrada ?? 0m;
            if (qty <= 0) return;

            var oc = await _context.Ocandreqs.FindAsync(entrada.IdOc);
            if (oc == null) return;

            var idDepto  = oc.IdDepartament;
            // Sucursal: en OC es id_reference; en CR el branch vive en la REQUIS padre (id_req).
            var idBranch = oc.IdReference;
            if (string.Equals(oc.Type, "CR", StringComparison.OrdinalIgnoreCase) && oc.IdReq.HasValue && oc.IdReq.Value > 0)
            {
                var idRef = await _context.Ocandreqs.AsNoTracking()
                    .Where(r => r.Id == oc.IdReq.Value).Select(r => r.IdReference).FirstOrDefaultAsync();
                if (idRef > 0) idBranch = idRef;
            }

            // Empresa robusta: IdRoot de la OC; si 0, empresa de la sucursal (Branchs).
            int idCompany = oc.IdRoot ?? 0;
            if (idCompany == 0 && idBranch > 0)
            {
                var conn = _context.Database.GetDbConnection();
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                using var cmd = conn.CreateCommand();
                cmd.Transaction = _context.Database.CurrentTransaction?.GetDbTransaction();
                cmd.CommandText = "SELECT id_company FROM smp.dbo.Branchs WHERE id = @id";
                var p = cmd.CreateParameter(); p.ParameterName = "@id"; p.Value = idBranch;
                cmd.Parameters.Add(p);
                var res = await cmd.ExecuteScalarAsync();
                if (res != null && res != System.DBNull.Value) idCompany = System.Convert.ToInt32(res);
            }
            if (idCompany <= 0 || idBranch <= 0) return;

            var mat = entrada.IdMaterial.Value;
            var inv = await _context.InventarioMp.FirstOrDefaultAsync(x =>
                x.IdCompany == idCompany && x.IdSucursal == idBranch &&
                x.IdDepartamento == idDepto && x.IdMaterial == mat);
            if (inv == null)
            {
                _context.InventarioMp.Add(new InventarioMpDelison
                {
                    IdCompany = idCompany, IdSucursal = idBranch, IdDepartamento = idDepto,
                    IdMaterial = mat, Cantidad = qty, Active = true, DateModified = DateTime.Now
                });
            }
            else
            {
                inv.Cantidad += qty;
                inv.DateModified = DateTime.Now;
            }
        }

        // ── Captura de Gastos: ingresar una entrada "A CRÉDITO" (Opción A) ──
        // El material YA ingresa al almacén global (inventario_mp) al activar el crédito, pero la
        // entrada NO se libera (liberacion sigue en 0) para que permanezca en Captura como PENDIENTE
        // DE PAGO a N días. Al pagarla luego (ConfirmPayment) NO se vuelve a sumar (guarda por Credito).
        public async Task<bool> ActivarCredito(ActivarCreditoDto dto)
        {
            if (dto == null || dto.IdEntrada <= 0) return false;
            var entrada = await _context.EntradasMolienda.FindAsync(dto.IdEntrada);
            if (entrada == null) return false;
            if (entrada.Liberacion) return true;   // ya pagada → nada que hacer
            if (entrada.Credito) return true;      // ya a crédito → no re-sumar al almacén (idempotente)

            entrada.Credito      = true;           // marca "ingresada a crédito" (separado de liberacion)
            // Persiste la fecha de vencimiento calculada en frontend (fechaRecepcion + N días).
            // Esto permite que el usuario la edite posteriormente desde Captura de Gastos.
            if (dto.FechaVencimiento.HasValue)
                entrada.FechaVencimiento = DateOnly.FromDateTime(dto.FechaVencimiento.Value);
            // Persiste tipo y número de documento si vienen del frontend.
            if (!string.IsNullOrWhiteSpace(dto.NotaFactura))    entrada.NotaFactura    = dto.NotaFactura;
            if (!string.IsNullOrWhiteSpace(dto.NumNotaFactura)) entrada.NumNotaFactura = dto.NumNotaFactura;
            entrada.DateModified = DateTime.UtcNow;

            // A crédito → el material entra al almacén global ahora (no al pagar).
            await AcumularInventarioMp(entrada);

            // ── Replica en Cuentas x Pagar del proveedor (proveedorxtablas type='CUENTA') ──
            // Solo si se conoce el proveedor (IdProvider > 0 en la OC).
            var oc = await _context.Ocandreqs.AsNoTracking().FirstOrDefaultAsync(o => o.Id == entrada.IdOc);
            if (oc != null && (oc.IdProvider ?? 0) > 0)
            {
                // Obtiene la fecha de la OC y de la requisición para los campos de fecha.
                var fechaOc  = oc.DateModified;
                var fechaReq = oc.DateModified; // fallback; se intenta refinar con la REQUIS padre
                var reqOc = await _context.Ocandreqs.AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == oc.IdReq);
                if (reqOc != null) fechaReq = reqOc.DateModified;

                // Calcula el monto: usa entrada.Pago si > 0, si no precio × cantidad del detalle.
                decimal monto = entrada.Pago ?? 0m;
                if (monto == 0m)
                {
                    var det = await _context.Detailsreqoc.AsNoTracking()
                        .FirstOrDefaultAsync(d => d.IdMovement == oc.Id && d.IdSupplie == (entrada.IdMaterial ?? 0) && d.Active == true);
                    if (det != null)
                        monto = det.Price * (entrada.CantidadEntrada ?? 0m);
                }

                _context.ProveedorXTablas.Add(new ProveedorXTabla
                {
                    IdTabla  = oc.IdProvider ?? 0,
                    Campo2   = entrada.NotaFactura ?? "",          // Tipo Factura/Nota
                    Campo11  = entrada.NumNotaFactura ?? "",       // Numero Factura/Nota (texto)
                    Campo3   = fechaReq.ToString("yyyy-MM-dd"),   // Fecha Requisicion
                    Campo8   = fechaOc,                            // Fecha OC
                    Campo4   = monto.ToString("F2"),               // Total por nota
                    Campo5   = "0",                                // Abono a Cuenta (inicial 0)
                    Campo6   = monto.ToString("F2"),               // Restante (= total inicial)
                    Type     = "CUENTA",
                    Active   = true,
                    Vigente  = true,
                    Principal = false,
                    Campo1   = 0,
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ── Grid de Órdenes de Compra: marcar el ANTICIPO de una OC como pagado ──
        // Registra el monto (= Total OC × cantidad%) y la fecha. A partir de aquí el saldo
        // del anticipo se va aplicando a las entradas al pagarlas en Gastos.
        public async Task<bool> MarcarAnticipo(MarcarAnticipoDto dto)
        {
            if (dto == null || dto.IdOc <= 0) return false;
            var oc = await _context.Ocandreqs.FindAsync(dto.IdOc);
            if (oc == null) return false;

            var fecha = dto.Fecha.HasValue
                ? DateOnly.FromDateTime(dto.Fecha.Value)
                : DateOnly.FromDateTime(DateTime.Now);

            // FASE 2: el anticipo entra "EN TRÁMITE" (encolado en Captura de Gastos), NO pagado.
            // anticipo_pagado se mantiene en false hasta que se pague en Captura → preserva la lógica
            // de saldo/neteo (el saldo del anticipo solo se aplica a entregas cuando ya salió el dinero).
            oc.AnticipoMonto = dto.Monto;
            oc.FechaAnticipo = fecha;
            if (oc.AnticipoEstado != "PAGADO")          // no degradar un anticipo ya pagado
                oc.AnticipoEstado = "EN_TRAMITE";

            // Crear/actualizar la línea de gasto (fuente de verdad para Captura/Reporte). Idempotente por OC.
            var gasto = await _context.GastosGenerales
                .FirstOrDefaultAsync(g => g.IdOc == dto.IdOc && g.TipoGasto == "ANTICIPO" && g.Active);

            if (gasto == null)
            {
                // Empresa robusta: IdRoot de la OC; si 0, empresa de la sucursal (Branchs).
                int idCompany = oc.IdRoot ?? 0;
                int idBranch  = oc.IdReference;
                if (idCompany == 0 && idBranch > 0)
                {
                    var conn = _context.Database.GetDbConnection();
                    if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT id_company FROM smp.dbo.Branchs WHERE id = @id";
                    var p = cmd.CreateParameter(); p.ParameterName = "@id"; p.Value = idBranch;
                    cmd.Parameters.Add(p);
                    var res = await cmd.ExecuteScalarAsync();
                    if (res != null && res != DBNull.Value) idCompany = Convert.ToInt32(res);
                }

                _context.GastosGenerales.Add(new GastosGeneralesDelison
                {
                    IdCompany     = idCompany,
                    IdBranch      = idBranch,
                    IdDepartament = oc.IdDepartament,
                    IdOc          = oc.Id,
                    TipoGasto     = "ANTICIPO",
                    Folio         = oc.Folio,                    // folio OC base (sin -E1)
                    Concepto      = $"Anticipo {oc.Folio}",
                    IdProvider    = oc.IdProvider,
                    Proveedor     = oc.Solicit,
                    Monto         = dto.Monto,
                    Estado        = "EN_TRAMITE",
                    FechaRegistro = fecha,
                    Active        = true,
                    DateModified  = DateTime.Now,
                });
            }
            else if (gasto.Estado != "PAGADO")
            {
                // Re-registro / edición de fecha o recálculo de monto antes de pagar.
                gasto.Monto         = dto.Monto;
                gasto.FechaRegistro = fecha;
                gasto.DateModified  = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ── Captura de Gastos: PAGAR un anticipo EN TRÁMITE ──
        // Marca el gasto general como PAGADO con la fecha del día del pago (la que cuenta para el reporte)
        // y actualiza la OC: anticipo_estado='PAGADO' + anticipo_pagado=true (habilita el saldo/neteo en entregas).
        public async Task<bool> ConfirmAnticipo(ConfirmAnticipoDto dto)
        {
            if (dto == null || dto.IdGastoGeneral <= 0) return false;

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var gasto = await _context.GastosGenerales.FindAsync(dto.IdGastoGeneral);
                if (gasto == null || !gasto.Active) { await tx.RollbackAsync(); return false; }
                if (gasto.Estado == "PAGADO") { await tx.RollbackAsync(); return true; } // idempotente

                var fecha = dto.FechaPago.HasValue
                    ? DateOnly.FromDateTime(dto.FechaPago.Value)
                    : DateOnly.FromDateTime(DateTime.Now);

                gasto.Estado      = "PAGADO";
                gasto.FechaPago   = fecha;
                if (!string.IsNullOrWhiteSpace(dto.NotaFactura)) gasto.NotaFactura = dto.NotaFactura;
                gasto.DateModified = DateTime.Now;

                // Fase 4: conversión a MXN del anticipo (moneda extranjera). MXN → TC=1.
                var tcAnt = dto.TipoCambio.HasValue && dto.TipoCambio.Value > 0 ? dto.TipoCambio.Value : 1m;
                gasto.Moneda     = string.IsNullOrWhiteSpace(dto.Moneda) ? "MXN" : dto.Moneda.Trim().ToUpperInvariant();
                gasto.TipoCambio = tcAnt;
                gasto.FuenteTc   = dto.FuenteTc;
                gasto.MontoMxn   = Math.Round(gasto.Monto * tcAnt, 2);

                // Reflejar en la OC para habilitar la lógica de saldo/neteo y el display PAGADO.
                if (gasto.IdOc.HasValue && gasto.IdOc.Value > 0)
                {
                    var oc = await _context.Ocandreqs.FindAsync(gasto.IdOc.Value);
                    if (oc != null)
                    {
                        oc.AnticipoEstado = "PAGADO";
                        oc.AnticipoPagado = true;          // ahora sí: el dinero salió → saldo disponible
                        oc.FechaAnticipo  = fecha;         // fecha de pago (la que se muestra read-only en Nivel 3)
                        if (!oc.AnticipoMonto.HasValue || oc.AnticipoMonto.Value <= 0)
                            oc.AnticipoMonto = gasto.Monto;
                    }
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Error confirmando pago de anticipo (gasto {IdGasto})", dto.IdGastoGeneral);
                throw;
            }
        }

        // ── Captura de Gastos: GUARDAR borrador (sin liberar, sin acumular cantidad) ──
        // Persiste solo los campos editables movidos. NO marca liberacion ni acumula sin límite
        // (eso ocurre únicamente al CONFIRMAR el pago).
        public async Task<bool> SavePending(ConfirmPaymentDto dto)
        {
            if (dto == null || dto.IdEntrada <= 0) return false;

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var entrada = await _context.EntradasMolienda.FindAsync(dto.IdEntrada);
                if (entrada == null) { await tx.RollbackAsync(); return false; }

                // Si ya está liberada, no se toca (ya se pagó).
                if (entrada.Liberacion) { await tx.RollbackAsync(); return true; }

                entrada.Pago           = dto.ValorPago;
                entrada.FechaPago      = dto.FechaPago.HasValue ? DateOnly.FromDateTime(dto.FechaPago.Value) : (DateOnly?)null;
                entrada.NotaFactura    = dto.NotaFactura;
                entrada.NumNotaFactura = dto.NumNotaFactura;
                // Persiste la fecha de vencimiento si el usuario la editó manualmente.
                if (dto.FechaVencimiento.HasValue)
                    entrada.FechaVencimiento = DateOnly.FromDateTime(dto.FechaVencimiento.Value);
                entrada.DateModified = DateTime.UtcNow;
                // NOTA: NO se toca entrada.Liberacion (sigue en false).

                // ¿Multi-entrega? El IVA es POR ENTREGA (entregas_oc.mas_iva), no del ítem OC compartido.
                bool esMultiEntrega = false;
                if (dto.IdEntrega.HasValue && dto.IdDetail.HasValue)
                {
                    var nEntregas = await _context.EntregasOc
                        .CountAsync(e => e.IdDetailsreqoc == dto.IdDetail.Value && e.Active);
                    esMultiEntrega = nEntregas > 1;
                }

                if (dto.IdDetail.HasValue)
                {
                    var d = await _context.Detailsreqoc.FindAsync(dto.IdDetail.Value);
                    if (d != null)
                    {
                        // En multi-entrega NO se toca el IVA del ítem compartido (va por entrega abajo).
                        if (!esMultiEntrega) d.MasIva = dto.MasIva;
                        if (string.Equals(dto.DocType, "CR", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!string.IsNullOrWhiteSpace(dto.Proveedor)) d.NameProvider = dto.Proveedor;
                            if (dto.PrecioUnitario.HasValue) d.Price = dto.PrecioUnitario.Value;
                            if (dto.IdCurrency.HasValue && dto.IdCurrency.Value > 0) d.IdCurrency = dto.IdCurrency.Value;
                            // Id del proveedor (detalle + documento CR) para componer el folio CR.
                            if (dto.IdProvider.HasValue && dto.IdProvider.Value > 0)
                            {
                                d.IdProvider = dto.IdProvider.Value;
                                var crOc = await _context.Ocandreqs.FindAsync(entrada.IdOc);
                                if (crOc != null) crOc.IdProvider = dto.IdProvider.Value;
                            }
                        }
                        // NOTA: NO se acumula Quantity (eso es solo al confirmar el pago).
                    }
                }

                if (dto.IdEntrega.HasValue)
                {
                    var eg = await _context.EntregasOc.FindAsync(dto.IdEntrega.Value);
                    if (eg != null)
                    {
                        if (!string.IsNullOrEmpty(dto.NotaFactura)) eg.NotaFactura = dto.NotaFactura;
                        if (esMultiEntrega) eg.MasIva = dto.MasIva;   // IVA por entrega (draft)
                    }
                }

                // NO se propaga al comparador (slot COTIZ): es una foto del momento, no multi-entrega.

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Error guardando borrador de la entrada {IdEntrada}", dto.IdEntrada);
                throw;
            }
        }

        // ── (Eliminado) PropagateMasIvaToCotizAsync ──
        // El comparador (slot COTIZ) ya NO se actualiza desde Gastos: es una foto del momento de la
        // comparación y no soporta multi-entrega (mostraría toda la cantidad con un IVA único). El IVA
        // vive por entrega (entregas_oc.mas_iva) y el costo real está en la OC y el almacén.

        // ── Helpers de lectura defensiva por nombre de columna ──
        private static int GetInt(System.Data.Common.DbDataReader r, string col) { var i = r.GetOrdinal(col); return r.IsDBNull(i) ? 0 : Convert.ToInt32(r.GetValue(i)); }
        private static int? GetIntOrNull(System.Data.Common.DbDataReader r, string col) { var i = r.GetOrdinal(col); return r.IsDBNull(i) ? (int?)null : Convert.ToInt32(r.GetValue(i)); }
        private static decimal GetDec(System.Data.Common.DbDataReader r, string col) { var i = r.GetOrdinal(col); return r.IsDBNull(i) ? 0m : Convert.ToDecimal(r.GetValue(i)); }
        private static decimal? GetDecOrNull(System.Data.Common.DbDataReader r, string col) { var i = r.GetOrdinal(col); return r.IsDBNull(i) ? (decimal?)null : Convert.ToDecimal(r.GetValue(i)); }
        private static bool GetBool(System.Data.Common.DbDataReader r, string col) { var i = r.GetOrdinal(col); return !r.IsDBNull(i) && Convert.ToBoolean(r.GetValue(i)); }
        private static string GetStr(System.Data.Common.DbDataReader r, string col) { var i = r.GetOrdinal(col); return r.IsDBNull(i) ? string.Empty : r.GetValue(i).ToString() ?? string.Empty; }
        private static string? GetStrOrNull(System.Data.Common.DbDataReader r, string col) { var i = r.GetOrdinal(col); return r.IsDBNull(i) ? null : r.GetValue(i).ToString(); }
        private static DateTime? GetDateOrNull(System.Data.Common.DbDataReader r, string col) { var i = r.GetOrdinal(col); return r.IsDBNull(i) ? (DateTime?)null : Convert.ToDateTime(r.GetValue(i)); }

        private static void AddParam(System.Data.Common.DbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            cmd.Parameters.Add(p);
        }
    }
}
