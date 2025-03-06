using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Warehouse.Models
{
    [Table("processxpermission")]
    public class ProcessXPermission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("id_procces")]
        public int? IdProcces { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("select")]
        public bool? Select { get; set; } = false;

        [Column("active")]
        public bool? Active { get; set; } = true;
    }
}