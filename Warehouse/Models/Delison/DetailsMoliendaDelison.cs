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

        [Column("type")]
        [MaxLength(10)]
        public string Type { get; set; } = string.Empty;

        [Column("fecha")]
        public DateOnly? Fecha { get; set; }

        [Column("cantidad")]
        public decimal Cantidad { get; set; }

        [Column("id_catalog")]
        public int? IdCatalog { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;
    }
}
