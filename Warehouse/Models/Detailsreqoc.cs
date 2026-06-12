
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models
{
    [Table("detailsreqoc")]
    public class Detailsreqoc
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_movement")]
        public int IdMovement { get; set; }

        [Column("id_supplie")]
        public int IdSupplie { get; set; } = 0;

        [Column("id_provider")]
        public int? IdProvider { get; set; } = 0;

        [Column("name_provider")]
        public string? NameProvider { get; set; }

        [Column("quantity", TypeName = "decimal(16,2)")]
        public decimal Quantity { get; set; }

        [Column("price", TypeName = "decimal(16,2)")]
        public decimal Price { get; set; }

        // Columna calculada en SQL: ([quantity] * [price])
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column("total", TypeName = "decimal(16,2)")]
        public decimal Total { get; private set; }

        [Column("dateuse", TypeName = "date")]
        public DateTime? Dateuse { get; set; }

        [Column("type", TypeName = "varchar(6)")]
        public string? Type { get; set; }

        [Column("recurrent", TypeName = "varchar(10)")]
        public string Recurrent { get; set; } = "Recurrente";

        [Column("namearticle", TypeName = "varchar(50)")]
        public string? NameArticle { get; set; }

        [Column("numarticle", TypeName = "varchar(20)")]
        public string? NumArticle { get; set; }

        [Column("description_new_article")]
        public string? DescriptionNewArticle { get; set; }

        [Column("url_new_article")]
        public string? UrlNewArticle { get; set; }

        [Column("justification_new_article")]
        public string? JustificationNewArticle { get; set; }

        [Column("intorext", TypeName = "varchar(10)")]
        public string Intorext { get; set; } = "Interno";

        [Column("provint", TypeName = "varchar(50)")]
        public string? ProvInt { get; set; } = "Sin Proveedor";

        [Column("typepriority", TypeName = "varchar(8)")]
        public string? TypePriority { get; set; } = "Normal";

        [Column("pedimento")]
        public bool? Pedimento { get; set; } = false;

        [Column("pedimentonum")]
        public string? PedimentoNum { get; set; }

        [Column("observation")]
        public string? Observation { get; set; }

        [Column("comment")]
        public string? Comment { get; set; }

        [Column("tiempoentrega")]
        public string? TiempoEntrega { get; set; }

        [Column("compraminima", TypeName = "decimal(16,2)")]
        public decimal? CompraMinima { get; set; }

        [Column("autorizado")]
        public bool? Autorizado { get; set; } = false;

        [Column("active")]
        public bool? Active { get; set; } = true;

        [Column("typeoc", TypeName = "varchar(50)")]
        public string? TypeOc { get; set; }

        [Column("datepostpone", TypeName = "date")]
        public DateTime? DatePostpone { get; set; }

        [Column("cantidadconceptualizada", TypeName = "decimal(16,2)")]
        public decimal? CantidadConceptualizada { get; set; }

        [Column("caducidad", TypeName = "varchar(40)")]
        public string? Caducidad { get; set; }

        [Column("caducidad_minima_requerida", TypeName = "varchar(255)")]
        public string? CaducidadMinimaRequerida { get; set; }

        [Column("comprarapida")]
        public bool? CompraRapida { get; set; } = false;

        [Column("mas_iva")]
        public bool? MasIva { get; set; } = false;

        [Column("dias_condicion_compra")]
        public int? DiasCondicionCompra { get; set; }

        [Column("datepostpone_confirmada")]
        public bool? DatePostponeConfirmada { get; set; } = false;

        // Proveedor que el panel de presentaciones sugirió en la requisición (para resaltarlo
        // en el dropdown de la cotización). NULL si no se usó el panel.
        [Column("id_proveedor_sugerido")]
        public int? IdProveedorSugerido { get; set; }

        // Moneda del precio del ítem. Se hereda del proveedor (proveedorxtablas.id_currency)
        // al tomar su costo en la COTIZ y viaja hasta la OC. NULL = MXN por default.
        // La conversión a pesos se hará al momento del pago (Gastos / anticipo), no aquí.
        [Column("id_currency")]
        public int? IdCurrency { get; set; }

        // Liberar para almacén: cuando true, el almacén del departamento que pidió la OC puede
        // leer/recibir este ítem. Las OC creadas en "Generar OC" nacen FALSE (gated hasta marcar el
        // check en Selección de OC); CR y datos existentes = TRUE (default). Gate en GetOcsByReqMaterial.
        [Column("liberar_almacen")]
        public bool LiberarAlmacen { get; set; } = true;
    }
}
