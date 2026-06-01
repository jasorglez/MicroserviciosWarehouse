using System.Data;
using Microsoft.EntityFrameworkCore;
using Warehouse.Models.DTOs;

namespace Warehouse.Service.Delison
{
    public interface IGastosService
    {
        Task<ExpenseReportDto> GetExpenseReport(int idCompany, DateTime startDate, DateTime endDate, string lens);
        Task<List<PendingPaymentDto>> GetPendingPayments(int idCompany);
        Task<List<PendingPaymentDto>> GetPaidPayments(int idCompany);
        Task<bool> ConfirmPayment(ConfirmPaymentDto dto);
        Task<bool> SavePending(ConfirmPaymentDto dto);
        Task<bool> ActivarCredito(int idEntrada);
        Task<bool> MarcarAnticipo(MarcarAnticipoDto dto);
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
                    SELECT
                        (CASE WHEN o.[type] = 'CR' THEN oreq.id_reference ELSE o.id_reference END) AS idBranch,
                        b.name                                                    AS branchName,
                        o.id_departament                                          AS idDepartament,
                        r.[Description]                                           AS departmentName,
                        SUM(CAST(ISNULL(e.pago, 0) AS DECIMAL(18,2)))             AS total,
                        COUNT(*)                                                  AS numTransacciones
                    FROM warehouses.Delison.entradas_molienda e
                    INNER JOIN warehouses.dbo.ocandreq o ON o.id = e.id_oc
                    -- En CR el branch está en la REQUIS padre (o.id_req), no en o.id_reference (ahí va el id del item origen).
                    LEFT  JOIN warehouses.dbo.ocandreq oreq ON oreq.id = o.id_req
                    INNER JOIN smp.dbo.Branchs b ON b.id = (CASE WHEN o.[type] = 'CR' THEN oreq.id_reference ELSE o.id_reference END)
                    LEFT  JOIN security.dbo.Roles r ON r.id = o.id_departament
                    WHERE e.active = 1
                      AND e.liberacion = 1            -- solo entradas YA pagadas/liberadas en Gastos
                      AND b.id_company = @idCompany
                      AND e.fecha_pago >= @start       -- ancla = fecha en que se confirmó el pago
                      AND e.fecha_pago <= @end
                    GROUP BY (CASE WHEN o.[type] = 'CR' THEN oreq.id_reference ELSE o.id_reference END), b.name, o.id_departament, r.[Description]"
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
                    COALESCE(NULLIF(e.folio_entrega, ''), o.folio) AS folio,
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
                    ISNULL(e.pago, 0)                            AS valorPago,
                    ISNULL(d.mas_iva, 0)                         AS masIva,
                    COALESCE(NULLIF(e.nota_factura, ''), eg.nota_factura) AS notaFactura,
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
                WHERE e.active = 1
                  AND e.liberacion = 0
                  AND b.id_company = @idCompany
                  AND (
                        (e.id_entrega IS NOT NULL AND eg.[close] = 1)
                     OR (e.id_entrega IS NULL AND d.typeoc = 'COMPRA AUTORIZADA SIN LIMITE' AND e.[close] = 1)
                     OR (e.id_entrega IS NULL AND o.[type] = 'CR'
                         AND EXISTS (SELECT 1 FROM warehouses.Delison.entregas_oc eg2 WHERE eg2.id_detailsreqoc = d.id AND eg2.active = 1 AND eg2.[close] = 1))
                     OR (e.id_entrega IS NULL AND o.[type] = 'OC' AND (d.typeoc IS NULL OR d.typeoc <> 'COMPRA AUTORIZADA SIN LIMITE')
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

                using var reader = await cmd.ExecuteReaderAsync();
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
                        MasIva         = GetBool(reader, "masIva"),
                        NotaFactura    = GetStrOrNull(reader, "notaFactura"),
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
                    COALESCE(NULLIF(e.folio_entrega, ''), o.folio) AS folio,
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
                    ISNULL(e.pago, 0)                            AS valorPago,
                    ISNULL(d.mas_iva, 0)                         AS masIva,
                    COALESCE(NULLIF(e.nota_factura, ''), eg.nota_factura) AS notaFactura,
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

                using var reader = await cmd.ExecuteReaderAsync();
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
                        MasIva         = GetBool(reader, "masIva"),
                        NotaFactura    = GetStrOrNull(reader, "notaFactura"),
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
                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo entradas pagadas. Company={IdCompany}", idCompany);
                throw;
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
                entrada.NotaFactura = dto.NotaFactura;
                entrada.Liberacion  = true;
                entrada.DateModified = DateTime.UtcNow;

                // 2) Writeback al detalle (detailsreqoc)
                if (dto.IdDetail.HasValue)
                {
                    var d = await _context.Detailsreqoc.FindAsync(dto.IdDetail.Value);
                    if (d != null)
                    {
                        d.MasIva = dto.MasIva; // IVA puede marcarse desde Gastos
                        if (string.Equals(dto.DocType, "CR", StringComparison.OrdinalIgnoreCase))
                        {
                            // Compra rápida: rellenar proveedor y precio unitario
                            if (!string.IsNullOrWhiteSpace(dto.Proveedor)) d.NameProvider = dto.Proveedor;
                            if (dto.PrecioUnitario.HasValue) d.Price = dto.PrecioUnitario.Value;
                        }
                        if (string.Equals(dto.CloseSource, "SIN_LIMITE", StringComparison.OrdinalIgnoreCase))
                        {
                            // Sin límite: acumular la cantidad de esta entrada en el detalle (Total se recalcula en SQL)
                            d.Quantity += dto.Cantidad;
                        }
                    }
                }

                // 3) Sync de nota/factura a la entrega (multi-entrega)
                if (dto.IdEntrega.HasValue && !string.IsNullOrEmpty(dto.NotaFactura))
                {
                    var eg = await _context.EntregasOc.FindAsync(dto.IdEntrega.Value);
                    if (eg != null) eg.NotaFactura = dto.NotaFactura;
                }

                // 4) Mantener el "+ IVA" consistente en la cotización/comparador (slot COTIZ origen)
                await PropagateMasIvaToCotizAsync(entrada.IdOc, entrada.IdMaterial, dto.MasIva);

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

        // ── Captura de Gastos: ingresar una entrada "A CRÉDITO" ──
        // El material queda disponible (placeholder: el almacén global aún no existe → solo estado)
        // y la entrada permanece como PENDIENTE DE PAGO a N días (no se libera). Al vencimiento se paga.
        public async Task<bool> ActivarCredito(int idEntrada)
        {
            if (idEntrada <= 0) return false;
            var entrada = await _context.EntradasMolienda.FindAsync(idEntrada);
            if (entrada == null) return false;
            if (entrada.Liberacion) return true;   // ya pagada → nada que hacer

            entrada.Credito      = true;           // marca "ingresada a crédito" (separado de liberacion)
            entrada.DateModified = DateTime.UtcNow;
            // TODO[almacén-global]: cuando exista, crear aquí el movimiento de entrada (inandout).
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

            oc.AnticipoPagado = true;
            oc.AnticipoMonto  = dto.Monto;
            oc.FechaAnticipo  = dto.Fecha.HasValue
                ? DateOnly.FromDateTime(dto.Fecha.Value)
                : DateOnly.FromDateTime(DateTime.Now);
            await _context.SaveChangesAsync();
            return true;
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

                entrada.Pago        = dto.ValorPago;
                entrada.FechaPago   = dto.FechaPago.HasValue ? DateOnly.FromDateTime(dto.FechaPago.Value) : (DateOnly?)null;
                entrada.NotaFactura = dto.NotaFactura;
                entrada.DateModified = DateTime.UtcNow;
                // NOTA: NO se toca entrada.Liberacion (sigue en false).

                if (dto.IdDetail.HasValue)
                {
                    var d = await _context.Detailsreqoc.FindAsync(dto.IdDetail.Value);
                    if (d != null)
                    {
                        d.MasIva = dto.MasIva;
                        if (string.Equals(dto.DocType, "CR", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!string.IsNullOrWhiteSpace(dto.Proveedor)) d.NameProvider = dto.Proveedor;
                            if (dto.PrecioUnitario.HasValue) d.Price = dto.PrecioUnitario.Value;
                        }
                        // NOTA: NO se acumula Quantity (eso es solo al confirmar el pago).
                    }
                }

                if (dto.IdEntrega.HasValue && !string.IsNullOrEmpty(dto.NotaFactura))
                {
                    var eg = await _context.EntregasOc.FindAsync(dto.IdEntrega.Value);
                    if (eg != null) eg.NotaFactura = dto.NotaFactura;
                }

                // Mantener el "+ IVA" consistente en la cotización/comparador (slot COTIZ origen)
                await PropagateMasIvaToCotizAsync(entrada.IdOc, entrada.IdMaterial, dto.MasIva);

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

        // ── Propaga el masIva capturado en Gastos de vuelta al detalle del slot COTIZ origen ──
        // La OC no referencia directamente su COTIZ; el único vínculo es IdReq (REQUIS) + IdProvider.
        // Tanto la cotización (getReqItems del slot) como el comparador (GetComparisonData) leen
        // mas_iva del detalle del slot COTIZ, así que al actualizarlo aquí el "+ IVA" queda igual
        // en cotización, comparador y gastos (mismo costo en todos lados). Corre dentro de la misma
        // transacción → se persiste con el SaveChangesAsync del llamador.
        private async Task PropagateMasIvaToCotizAsync(int idOc, int? idMaterial, bool masIva)
        {
            if (idOc <= 0 || !idMaterial.HasValue || idMaterial.Value <= 0) return;

            var oc = await _context.Ocandreqs.FindAsync(idOc);
            if (oc == null
                || !oc.IdReq.HasValue || oc.IdReq.Value <= 0
                || !oc.IdProvider.HasValue || oc.IdProvider.Value <= 0) return;

            // Slots COTIZ del mismo proveedor bajo la misma requisición
            var slotIds = await _context.Ocandreqs
                .Where(c => c.Type == "COTIZ" && c.Active == true
                            && c.IdReq == oc.IdReq && c.IdProvider == oc.IdProvider)
                .Select(c => c.Id)
                .ToListAsync();
            if (slotIds.Count == 0) return;

            // Detalle del mismo material dentro de esos slots
            var slotDetails = await _context.Detailsreqoc
                .Where(d => slotIds.Contains(d.IdMovement)
                            && d.IdSupplie == idMaterial.Value && d.Active == true)
                .ToListAsync();

            foreach (var sd in slotDetails) sd.MasIva = masIva;
        }

        // ── Helpers de lectura defensiva por nombre de columna ──
        private static int GetInt(System.Data.Common.DbDataReader r, string col) { var i = r.GetOrdinal(col); return r.IsDBNull(i) ? 0 : Convert.ToInt32(r.GetValue(i)); }
        private static int? GetIntOrNull(System.Data.Common.DbDataReader r, string col) { var i = r.GetOrdinal(col); return r.IsDBNull(i) ? (int?)null : Convert.ToInt32(r.GetValue(i)); }
        private static decimal GetDec(System.Data.Common.DbDataReader r, string col) { var i = r.GetOrdinal(col); return r.IsDBNull(i) ? 0m : Convert.ToDecimal(r.GetValue(i)); }
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
