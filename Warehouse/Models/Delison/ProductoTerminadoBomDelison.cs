using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Delison
{
    /// <summary>
    /// Explosion de materiales (BOM) de un producto terminado. Lista de adyacencia:
    /// cada fila apunta a su padre (IdPadre). Raiz = producto terminado (IdPadre NULL).
    /// Profundidad variable (padre -> hijo -> nieto...). Costos suben por roll-up.
    /// </summary>
    [Table("producto_terminado_bom", Schema = "Delison")]
    public class ProductoTerminadoBomDelison
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("id_company")]
        public int IdCompany { get; set; }

        // vw_finalproduct.id: producto terminado al que pertenece todo el arbol.
        [Column("id_producto_root")]
        public int IdProductoRoot { get; set; }

        // FK auto-referenciada a Id. NULL = fila raiz (el producto terminado).
        [Column("id_padre")]
        public int? IdPadre { get; set; }

        // Material del catalogo (hoja basica o semi-elaborado). NULL en la raiz.
        [Column("id_material")]
        public int? IdMaterial { get; set; }

        [Column("nombre")]
        [MaxLength(150)]
        public string? Nombre { get; set; }

        // true = hoja basica (costo = ultima compra). false = semi-elaborado (costo calculado Capa 1).
        [Column("es_basico")]
        public bool EsBasico { get; set; } = false;

        [Column("cantidad", TypeName = "decimal(18,4)")]
        public decimal Cantidad { get; set; } = 0;

        [Column("unidad")]
        [MaxLength(20)]
        public string? Unidad { get; set; }

        // % de merma / desperdicio.
        [Column("merma_pct", TypeName = "decimal(9,4)")]
        public decimal MermaPct { get; set; } = 0;

        // Cache: basico = ultima compra; semi-elaborado = costo_base de la Capa 1.
        [Column("costo_unitario", TypeName = "decimal(18,4)")]
        public decimal CostoUnitario { get; set; } = 0;

        // Cache: cantidad * costo_unitario * (1 + merma_pct/100).
        [Column("costo_total", TypeName = "decimal(18,4)")]
        public decimal CostoTotal { get; set; } = 0;

        [Column("orden")]
        public int Orden { get; set; } = 0;

        // Cache de profundidad en el arbol.
        [Column("nivel")]
        public int Nivel { get; set; } = 0;

        [Column("comentarios")]
        [MaxLength(200)]
        public string? Comentarios { get; set; }

        // Solo aplica a básicos: con qué base se costea. PONDERADO (default) | ULTIMA | MAXIMO.
        [Column("modo_costo")]
        [MaxLength(10)]
        public string ModoCosto { get; set; } = "PONDERADO";

        [Column("active")]
        public bool Active { get; set; } = true;

        [Column("datemodified")]
        public DateTime DateModified { get; set; } = DateTime.UtcNow;
    }
}
