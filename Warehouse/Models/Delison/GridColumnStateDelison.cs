using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Delison
{
    // Persistencia POR USUARIO del estado de columnas de cualquier AG Grid
    // (visibilidad + orden + ancho). Clave lógica = (id_user, grid_key).
    [Table("grid_column_state", Schema = "Delison")]
    public class GridColumnStateDelison
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("id_user")]
        public int IdUser { get; set; }

        // Identificador único del grid (ej. 'gastos-captura', 'almmolienda', ...)
        [Column("grid_key")]
        [MaxLength(100)]
        public string GridKey { get; set; } = string.Empty;

        // JSON del columnState de AG Grid (gridApi.getColumnState()).
        [Column("column_state")]
        public string? ColumnState { get; set; }

        [Column("datemodified")]
        public DateTime DateModified { get; set; } = DateTime.UtcNow;
    }
}
