using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Delison
{
    [Table("empaque_peso_volumen", Schema = "Delison")]
    public class EmpaquePesoVolumenDelison
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // FK -> warehouses.Delison.empaque_descripcion.id (la presentación)
        [Column("id_empaque")]
        public int IdEmpaque { get; set; }

        [Column("medida", TypeName = "decimal(10,2)")]
        public decimal? Medida { get; set; }

        [Column("id_unidad")]
        public int? IdUnidad { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;

        [Column("datemodified")]
        public DateTime DateModified { get; set; } = DateTime.Now;
    }
}
