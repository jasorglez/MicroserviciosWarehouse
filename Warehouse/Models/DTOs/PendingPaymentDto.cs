namespace Warehouse.Models.DTOs
{
    /// <summary>
    /// Un renglón de la Captura de Gastos: una ENTRADA lista para pagar
    /// (liberacion=0 y su cierre de nivel activo). Incluye el contexto de
    /// documento/sucursal/departamento y los datos editables/de writeback.
    /// </summary>
    public class PendingPaymentDto
    {
        public int IdEntrada { get; set; }
        public int IdOc { get; set; }
        public string Folio { get; set; } = string.Empty;
        public string DocType { get; set; } = string.Empty;   // "OC" | "CR"
        public string? TipoOc { get; set; }                   // detailsreqoc.typeoc (o null en CR)
        public string CloseSource { get; set; } = string.Empty; // "ENTREGA" | "SIN_LIMITE" | "OC"

        public int IdReference { get; set; }                  // sucursal
        public string BranchName { get; set; } = string.Empty;
        public int IdDepartament { get; set; }
        public string DepartmentName { get; set; } = string.Empty;

        public int? IdMaterial { get; set; }
        public string Articulo { get; set; } = string.Empty;
        public string? NumArticulo { get; set; }              // detailsreqoc.numarticle (código MPxxYY-####)
        public int? IdDetail { get; set; }                    // detailsreqoc.id (para writeback)
        public int? IdEntrega { get; set; }                   // si multi-entrega

        // Editables / writeback
        public string? Proveedor { get; set; }
        public decimal Cantidad { get; set; }                 // entradas_molienda.cantidad_entrada
        public decimal CantidadOc { get; set; }               // detailsreqoc.quantity (cantidad ordenada en la OC)
        public decimal CantidadReq { get; set; }              // detailsreqoc.quantity de la REQUIS padre (cantidad requisitada)
        public decimal PrecioUnitario { get; set; }           // detailsreqoc.price
        public decimal ValorPago { get; set; }                // entradas_molienda.pago (default = lo de molienda)
        public bool MasIva { get; set; }                      // detailsreqoc.mas_iva
        public string? NotaFactura { get; set; }
        public string? NumNotaFactura { get; set; }

        public DateTime? FechaRecepcion { get; set; }
        public DateTime? FechaPago { get; set; }
        public DateTime? FechaVencimiento { get; set; }

        // ── Condición de pago (catálogo condiciones_pago vía ocandreq.id_condicion_pago) ──
        public bool CalculoAnticipo { get; set; }             // true = bloque ANTICIPO; false = bloque CRÉDITO
        public int CondicionCantidad { get; set; }            // crédito → N días; anticipo → % del total
        public bool Credito { get; set; }                     // entradas_molienda.credito (ya ingresada a crédito)

        // ── Estado del anticipo (a nivel OC) ──
        public bool AnticipoPagado { get; set; }              // el dinero del anticipo ya se entregó
        public decimal AnticipoMonto { get; set; }            // monto total del anticipo registrado
        public decimal AnticipoSaldo { get; set; }            // anticipo_monto − Σ anticipo_aplicado (entradas activas)
        public string? MetodoAnticipo { get; set; }           // 'FIFO' | 'PRORRATEO' (null hasta la 1ª aplicación)
        public int? NumProrrateo { get; set; }                // entregas para prorrateo (si ya se eligió)
        public int NumEntregasPlan { get; set; }              // entregas creadas para el detalle (default de prorrateo)
        public int NumEntradasAlmacen { get; set; }           // entradas reales en entradas_molienda para esta OC+material

        // ── Gasto general (no-material): anticipo, servicio, nómina, impuesto… ──
        // Cuando viene poblado, la fila NO es una entrada de almacén sino un gasto de
        // Delison.gastos_generales. DocType='ANTICIPO' (u otro tipo) y el pago se confirma
        // por su propio flujo (no por entradas_molienda).
        public int? IdGastoGeneral { get; set; }

        // Desglose de los artículos de la OC detrás de un anticipo (para el tooltip de la columna Artículo).
        public List<AnticipoItemDto>? AnticipoItems { get; set; }

        // Consumo del anticipo por entrega (para el tooltip de la columna Valor en la fila de anticipo).
        public List<AnticipoConsumoDto>? AnticipoConsumo { get; set; }

        // Monto del anticipo de la OC aplicado a ESTA entrada (para mostrar el neto en el histórico).
        public decimal AnticipoAplicado { get; set; }

        // Porcentaje ORIGINAL con que se calculó el anticipo (condiciones_pago.cantidad). NO recalculado por IVA.
        public decimal AnticipoPorcentaje { get; set; }

        // ── Fase 4: info de conversión a MXN (histórico ya pagado). ValorPago ya viene en MXN. ──
        public decimal? TipoCambio { get; set; }   // TC aplicado al pagar (null/1 = MXN)
        public string? Moneda { get; set; }        // ISO de la moneda original (MXN/USD/EUR)
        public string? FuenteTc { get; set; }      // BANXICO | RESPALDO | MANUAL
    }

    /// <summary>Un artículo de la OC sobre la que se hizo el anticipo (para el tooltip de desglose).</summary>
    public class AnticipoItemDto
    {
        public string Articulo { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }   // base o base+IVA (round2), igual que se muestra
        public decimal Total { get; set; }            // PrecioUnitario(round2) × Cantidad
    }

    /// <summary>Una entrega que consumió parte del anticipo (para el tooltip de consumo en la fila de anticipo).</summary>
    public class AnticipoConsumoDto
    {
        public string FolioEntrega { get; set; } = string.Empty;
        public decimal Descuento { get; set; }        // entradas_molienda.anticipo_aplicado de esa entrega
    }

    /// <summary>
    /// Cuerpo para confirmar el pago de UNA entrada desde la Captura de Gastos.
    /// Dispara (transaccional): pago + fecha_pago + nota_factura + liberacion en la entrada;
    /// writeback a detailsreqoc (IVA siempre; proveedor/precio si es CR; acumula cantidad si sin límite);
    /// sync de nota_factura a la entrega si aplica.
    /// </summary>
    public class ConfirmPaymentDto
    {
        public int IdEntrada { get; set; }
        public int? IdDetail { get; set; }
        public int? IdEntrega { get; set; }
        public string DocType { get; set; } = string.Empty;     // "OC" | "CR"
        public string CloseSource { get; set; } = string.Empty;  // "ENTREGA" | "SIN_LIMITE" | "OC"

        public decimal ValorPago { get; set; }
        public DateTime? FechaPago { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string? Proveedor { get; set; }
        public decimal? PrecioUnitario { get; set; }
        public bool MasIva { get; set; }
        public string? NotaFactura { get; set; }
        public string? NumNotaFactura { get; set; }
        public decimal Cantidad { get; set; }                    // para acumular en sin límite

        // ── Aplicación de anticipo al confirmar/guardar (bloque ANTICIPO) ──
        public decimal? AnticipoAplicado { get; set; }           // monto del anticipo aplicado a esta entrada
        public string? MetodoAnticipo { get; set; }              // 'FIFO' | 'PRORRATEO' (se fija en la 1ª entrada)
        public int? NumProrrateo { get; set; }                   // entregas elegidas para prorrateo

        // ── Fase 4: conversión a MXN (moneda extranjera). El backend persiste monto_mxn = ValorPago × TipoCambio. ──
        public decimal? TipoCambio { get; set; }                 // pesos por unidad de la moneda (MXN=1)
        public string? Moneda { get; set; }                      // ISO: MXN | USD | EUR
        public string? FuenteTc { get; set; }                    // BANXICO | RESPALDO | MANUAL
    }

    /// <summary>Marca el anticipo de una OC como pagado (desde el grid de Órdenes de Compra).</summary>
    public class MarcarAnticipoDto
    {
        public int IdOc { get; set; }
        public decimal Monto { get; set; }            // monto del anticipo (= Total OC × cantidad%)
        public DateTime? Fecha { get; set; }
    }

    /// <summary>Confirma el pago de un anticipo EN TRÁMITE desde la Captura de Gastos.
    /// Marca el gasto general como PAGADO (con la fecha del día que se pagó) y actualiza la OC
    /// (anticipo_estado='PAGADO', anticipo_pagado=true) para que su saldo se aplique a las entregas.</summary>
    public class ConfirmAnticipoDto
    {
        public int IdGastoGeneral { get; set; }
        public DateTime? FechaPago { get; set; }
        public string? NotaFactura { get; set; }

        // Fase 4: conversión a MXN del anticipo. monto_mxn = gasto.Monto × TipoCambio.
        public decimal? TipoCambio { get; set; }
        public string? Moneda { get; set; }
        public string? FuenteTc { get; set; }
    }

    /// <summary>Ingresa una entrada "a crédito" (material disponible, pago pendiente a N días).</summary>
    public class ActivarCreditoDto
    {
        public int IdEntrada { get; set; }
        /// <summary>Fecha de vencimiento calculada en el frontend (fechaRecepcion + N días). Se persiste en BD para permitir edición posterior.</summary>
        public DateTime? FechaVencimiento { get; set; }
        /// <summary>Tipo de documento: "Nota" o "Factura". Se persiste en entradas_molienda y se replica a proveedorxtablas (campo2).</summary>
        public string? NotaFactura { get; set; }
        /// <summary>Número / folio del documento. Se persiste en entradas_molienda y se replica a proveedorxtablas (campo11).</summary>
        public string? NumNotaFactura { get; set; }
    }
}
