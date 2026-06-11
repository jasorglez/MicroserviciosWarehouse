using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Delison
{
    // Características definidas por material en la hoja de Materia Prima (materiales-maestro).
    // Pueblan la pestaña "Características de materia prima" en almacén molienda (las Activo=true).
    [Table("caracteristicas_materia_prima", Schema = "Delison")]
    public class CaracteristicasMateriaPrimaDelison
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("id_material")]
        public int IdMaterial { get; set; }

        // Checkbox "Activo" (negocio): si la característica se considera vigente para revisar.
        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Column("caracteristica")]
        [MaxLength(150)]
        public string? Caracteristica { get; set; }

        // Estado del registro (soft delete).
        [Column("active")]
        public bool Active { get; set; } = true;

        [Column("datemodified")]
        public DateTime DateModified { get; set; } = DateTime.UtcNow;
    }
}
