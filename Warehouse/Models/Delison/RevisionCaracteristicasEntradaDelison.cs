using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Delison
{
    // Revisión de características por ENTRADA (almacén molienda): un registro por
    // (entrada × característica de materia prima). reviso + comentarios por característica;
    // id_trabajador = revisor de la entrada (mismo en todas las filas de esa entrada).
    [Table("revision_caracteristicas_entrada", Schema = "Delison")]
    public class RevisionCaracteristicasEntradaDelison
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("id_entrada")]
        public int IdEntrada { get; set; }

        [Column("id_caracteristica")]
        public int IdCaracteristica { get; set; }

        [Column("reviso")]
        public bool Reviso { get; set; } = false;

        [Column("comentarios")]
        [MaxLength(250)]
        public string? Comentarios { get; set; }

        // Revisor (empleado del depto Extracción y Fermentación). Mismo valor para toda la entrada.
        [Column("id_trabajador")]
        public int? IdTrabajador { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;

        [Column("datemodified")]
        public DateTime DateModified { get; set; } = DateTime.UtcNow;
    }
}
