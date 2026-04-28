using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Delison
{
    [Table("detailsmolienda", Schema = "Delison")]
    public class DetailsMoliendaDelison
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("id_molienda")]
        public int IdMolienda { get; set; }

        [Column("id_requisition")]
        public int? IdRequisition { get; set; }

        [Column("type")]
        [MaxLength(10)]
        public string Type { get; set; } = string.Empty;

        [Column("cantidadreq", TypeName = "decimal(12,2)")]
        public decimal? CantidadReq { get; set; }

        [Column("numcantidadoc")]
        public int? NumCantidadOc { get; set; }

        [Column("fecha")]
        public DateOnly? Fecha { get; set; }

        [Column("id_catalog")]
        public int? IdCatalog { get; set; }

        [Column("cantidad", TypeName = "decimal(18,4)")]
        public decimal Cantidad { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;
    }
}
