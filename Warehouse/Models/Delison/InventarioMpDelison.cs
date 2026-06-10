using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Delison
{
    /// <summary>
    /// Almacén GLOBAL de materia prima: existencia por sucursal + departamento + material.
    /// Se suma con las entradas liberadas (pagadas en la Hoja de Gastos). Hoy la única fuente es
    /// el flujo de Molienda (departamento Extracción y Fermentación). Las salidas (consumo de
    /// producción) se restarán a futuro. Es de solo lectura para el usuario; lo alimenta el backend.
    /// </summary>
    [Table("inventario_mp", Schema = "Delison")]
    public class InventarioMpDelison
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("id_company")]
        public int IdCompany { get; set; }

        [Column("id_sucursal")]
        public int IdSucursal { get; set; }

        [Column("id_departamento")]
        public int IdDepartamento { get; set; }

        [Column("id_material")]
        public int IdMaterial { get; set; }

        // Existencia actual: += cantidad de la entrada al liberar; -= al consumir (futuro).
        [Column("cantidad", TypeName = "decimal(18,2)")]
        public decimal Cantidad { get; set; } = 0m;

        [Column("active")]
        public bool Active { get; set; } = true;

        [Column("datemodified")]
        public DateTime DateModified { get; set; } = DateTime.Now;
    }
}
