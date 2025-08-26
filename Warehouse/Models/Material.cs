using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Warehouse.Models
{
    [Table("materials")]
    public class Material
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_company")]
        [DefaultValue(1)]
        public int? IdCompany { get; set; }

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

        [Column("id_branch")]
        public int? IdBranch { get; set; }

        [Column("id_category")]
        public int? IdCategory { get; set; }

        [Column("id_familia")]
        public int? IdFamilia { get; set; }

        [Column("id_subfamilia")]
        [DefaultValue(1)]
        public int? IdSubfamilia { get; set; }

        [Column("id_medida")]
        public int? IdMedida { get; set; }

        [Column("id_ubication")]
        public int? IdUbication { get; set; }

        [Column("id_typematerial")]
        [DefaultValue(0)]
        public int? IdTypeMaterial { get; set; }

        [Column("description", TypeName = "VARCHAR(MAX)")]
        public string? Description { get; set; }

        [Column("date", TypeName = "DATE")]
        public DateTime? Date { get; set; }

        [Column("aplicaResg")]
        public bool? AplicaResg { get; set; }

        [Column("costoMN", TypeName = "DECIMAL(16,2)")]
        public decimal? CostoMN { get; set; }

        [Column("costoDLL", TypeName = "DECIMAL(16,2)")]
        public decimal? CostoDLL { get; set; }

        [Column("ventaMN", TypeName = "DECIMAL(16,2)")]
        public decimal? VentaMN { get; set; }

        [Column("ventaDLL", TypeName = "DECIMAL(16,2)")]
        public decimal? VentaDLL { get; set; }

        [Column("stockmin")]
        public int? StockMin { get; set; }

        [Column("stockmax")]
        public int? StockMax { get; set; }

        [Column("picture", TypeName = "VARCHAR")]
        [StringLength(250)]
        public string? Picture { get; set; }

        [Column("presentation")]
        public int? Presentation { get; set; }

        [Column("stock")]
        public int? Stock { get; set; }

        [Column("sellingprice")]
        public int? SellingPrice { get; set; }

        [Column("stockrequest")]
        public int? StockRequest { get; set; }

        [Column("barcodeZ", TypeName = "NCHAR")]
        [StringLength(250)]
        public string? BarcodeZ { get; set; }

        [Column("listprice", TypeName = "NCHAR")]
        [StringLength(10)]
        public string? ListPrice { get; set; }

        [Column("packagequantity", TypeName = "DECIMAL(10,2)")]
        public decimal? PackageQuantity { get; set; }

        [Column("packageprice", TypeName = "NCHAR")]
        [StringLength(10)]
        public string? PackagePrice { get; set; }

        [Column("descriptionpackage", TypeName = "VARCHAR")]
        [StringLength(100)]
        public string? DescriptionPackage { get; set; }

        [Column("measure", TypeName = "VARCHAR")]
        [StringLength(50)]
        public string? Measure { get; set; }

        [Column("requestquantity")]
        public int? RequestQuantity { get; set; }

        [Column("weightorvolumes", TypeName = "DECIMAL(10,2)")]
        [DefaultValue(0)]
        public decimal? WeightOrVolumes { get; set; }

        [Column("expiration")]
        [DefaultValue(0)]
        public int? Expiration { get; set; }

        [Column("typematerial", TypeName = "CHAR")]
        [StringLength(10)]
        [DefaultValue("CONSUMIBLE")]
        public string? TypeMaterial { get; set; }

        [Column("vigente")]
        [DefaultValue(true)]
        public bool? Vigente { get; set; }

        [Column("active")]
        [DefaultValue(true)]
        public bool? Active { get; set; }
    }
}