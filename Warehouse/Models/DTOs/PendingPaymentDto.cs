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
        public decimal PrecioUnitario { get; set; }           // detailsreqoc.price
        public decimal ValorPago { get; set; }                // entradas_molienda.pago (default = lo de molienda)
        public bool MasIva { get; set; }                      // detailsreqoc.mas_iva
        public string? NotaFactura { get; set; }

        public DateTime? FechaRecepcion { get; set; }
        public DateTime? FechaPago { get; set; }

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
        public string? Proveedor { get; set; }
        public decimal? PrecioUnitario { get; set; }
        public bool MasIva { get; set; }
        public string? NotaFactura { get; set; }
        public decimal Cantidad { get; set; }                    // para acumular en sin límite

        // ── Aplicación de anticipo al confirmar/guardar (bloque ANTICIPO) ──
        public decimal? AnticipoAplicado { get; set; }           // monto del anticipo aplicado a esta entrada
        public string? MetodoAnticipo { get; set; }              // 'FIFO' | 'PRORRATEO' (se fija en la 1ª entrada)
        public int? NumProrrateo { get; set; }                   // entregas elegidas para prorrateo
    }

    /// <summary>Marca el anticipo de una OC como pagado (desde el grid de Órdenes de Compra).</summary>
    public class MarcarAnticipoDto
    {
        public int IdOc { get; set; }
        public decimal Monto { get; set; }            // monto del anticipo (= Total OC × cantidad%)
        public DateTime? Fecha { get; set; }
    }

    /// <summary>Ingresa una entrada "a crédito" (material disponible, pago pendiente a N días).</summary>
    public class ActivarCreditoDto
    {
        public int IdEntrada { get; set; }
    }
}
