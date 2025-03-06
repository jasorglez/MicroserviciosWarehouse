using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [Column("select")]
        public int? Select { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;
    }
}
