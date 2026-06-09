using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Delison
{
    /// <summary>
    /// Caché diaria del tipo de cambio a MXN por moneda (Fase 4). Una fila por (moneda, fecha).
    /// Se llena al pagar: Banxico FIX como fuente principal, API gratuita de respaldo, o captura
    /// manual. Evita golpear las APIs en cada pago y deja histórico del TC usado.
    /// </summary>
    [Table("currency_rates", Schema = "Delison")]
    public class CurrencyRateDelison
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Código ISO de la moneda (USD, EUR, ...). MXN no se cachea (siempre 1).
        [Column("moneda", TypeName = "varchar(5)")]
        public string Moneda { get; set; } = "";

        [Column("fecha", TypeName = "date")]
        public DateOnly Fecha { get; set; }

        // Pesos por unidad de la moneda (ej. 17.30 MXN por 1 USD).
        [Column("tasa", TypeName = "decimal(18,6)")]
        public decimal Tasa { get; set; }

        // BANXICO | RESPALDO | MANUAL
        [Column("fuente", TypeName = "varchar(15)")]
        public string Fuente { get; set; } = "";

        [Column("datecreated")]
        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}
