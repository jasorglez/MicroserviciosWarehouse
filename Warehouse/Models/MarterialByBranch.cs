using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Warehouse.Models
{
    [Table("materialsByBranchVW")]
    public class MaterialsByBranchVW
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_company")]
        public int IdCompany { get; set; }

        [Column("id_branch")]
        public int? IdBranch { get; set; }

        [Column("id_customer")]
        public int? IdCustomer { get; set; }

        [Column("insumo", TypeName = "VARCHAR")]
        [StringLength(35)]
        public string? Insumo { get; set; }

        [Column("articulo", TypeName = "VARCHAR")]
        [StringLength(50)]
        public string? Articulo { get; set; }

        [Column("barcode", TypeName = "VARCHAR")]
        [StringLength(50)]
        public string? BarCode { get; set; }

        [Required]
        [Column("id_familia")]
        public int IdFamilia { get; set; }
        
        [Column("id_subfamilia")]
        public int? IdSubfamilia { get; set; }

        [Required]
        [Column("id_medida")]
        public int IdMedida { get; set; }

        [Required]
        [Column("id_ubication")]
        public int IdUbication { get; set; }

        [Column("description", TypeName = "VARCHAR(MAX)")]
        public string? Description { get; set; }

        [Column("date", TypeName = "DATE")]
        public DateTime? Date { get; set; }

        [Column("aplicaResg")]
        public bool? AplicaResg { get; set; }

        [Required]
        [Column("costoMN", TypeName = "DECIMAL(16,2)")]
        public decimal CostoMN { get; set; }

        [Required]
        [Column("costoDLL", TypeName = "DECIMAL(16,2)")]
        public decimal CostoDLL { get; set; }

        [Required]
        [Column("ventaMN", TypeName = "DECIMAL(16,2)")]
        public decimal VentaMN { get; set; }

        [Required]
        [Column("ventaDLL", TypeName = "DECIMAL(16,2)")]
        public decimal VentaDLL { get; set; }

        [Required]
        [Column("stockmin")]
        public int StockMin { get; set; }

        [Required]
        [Column("stockmax")]
        public int StockMax { get; set; }

        [Required]
        [Column("picture", TypeName = "NVARCHAR")]
        [StringLength(250)]
        public string Picture { get; set; } = string.Empty;

        [Column("typematerial", TypeName = "CHAR")]
        [StringLength(10)]
        public string TypeMaterial { get; set; } = string.Empty;

        [Column("vigente")]
        public bool Vigente { get; set; }

        [Required]
        [Column("active")]
        public bool Active { get; set; }
        
        // Propiedad de navegación para la relación inversa con PricesXProductsPresentation
        public virtual ICollection<PricesXProductsPresentation> PricesWithMaterial { get; set; } = new List<PricesXProductsPresentation>();
    }
}
