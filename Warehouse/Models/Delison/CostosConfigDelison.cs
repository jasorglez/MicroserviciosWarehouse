using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Delison
{
    /// <summary>
    /// Configuración de costeo por empresa. Por ahora: ventana móvil (meses)
    /// para el promedio ponderado de costos de materiales básicos.
    /// </summary>
    [Table("costos_config", Schema = "Delison")]
    public class CostosConfigDelison
    {
        [Key]
        [Column("id_company")]
        public int IdCompany { get; set; }

        [Column("ventana_meses")]
        public int VentanaMeses { get; set; } = 12;

        [Column("datemodified")]
        public DateTime DateModified { get; set; } = DateTime.UtcNow;
    }
}
