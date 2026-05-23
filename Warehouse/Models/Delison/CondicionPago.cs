using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Delison
{
    [Table("condiciones_pago")]
    public class CondicionPago
    {
        [Key]
        public int Id { get; set; }

        [Column("descripcion")]
        [StringLength(200)]
        public string Descripcion { get; set; } = string.Empty;

        [Column("cantidad")]
        public int Cantidad { get; set; } = 0;

        [Column("active")]
        public bool Active { get; set; } = true;

        [Column("id_company")]
        public int IdCompany { get; set; } = 9;

        [Column("datemodified")]
        public DateTime DateModified { get; set; } = DateTime.Now;
    }
}
