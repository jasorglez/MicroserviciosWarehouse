
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Warehouse.Models
{
    [Table("materialsxprovexist", Schema = "dbo")]
    public class MaterialsxProvExist
    {
        [Key]
        public int Id { get; set; }

        [Column("id_company")]
        public int? IdCompany { get; set; }

        [Column("id_branch")]
        public int? IdBranch { get; set; }

        [Column("typeOcorReq")]
        [StringLength(50)]
        public string? TypeOcorReq { get; set; }

        [Column("id_customer")]
        public int? IdCustomer { get; set; }

        [Column("typematerial")]
        [StringLength(10)]
        public string TypeMaterial { get; set; }

        [Column("insumo")]
        [StringLength(35)]
        public string? Insumo { get; set; }

        [Column("barcode")]
        [StringLength(50)]
        public string? Barcode { get; set; }

        [Column("company")]
        [StringLength(150)]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? Company { get; set; } = "NA";

        [Column("articulo")]
        [StringLength(50)]
        public string? Articulo { get; set; }

        [Column("id_category")]
        public int? IdCategory { get; set; }

        [Column("id_familia")]
        public int? IdFamilia { get; set; }

        [Column("id_subfamilia")]
        public int? IdSubfamilia { get; set; }

        [Column("description")]
        [StringLength(500)]
        public string? Description { get; set; }

        [Column("folio")]
        [StringLength(20)]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? Folio { get; set; }

        [Column("price", TypeName = "decimal(18,4)")]
        public decimal? Price { get; set; }

        [Column("quantity", TypeName = "decimal(18,4)")]
        public decimal? Quantity { get; set; }

        [Column("id_medida")]
        public int? IdMedida { get; set; }

        [Column("id_ubication")]
        public int? IdUbication { get; set; }

        [Column("date")]
        public DateTime? Date { get; set; }

        [Column("aplicaResg")]
        public bool? AplicaResg { get; set; }

        [Column("costoMN",TypeName = "decimal(18,4)")]
        public decimal? CostoMN { get; set; }

        [Column("costoDLL",TypeName = "decimal(18,4)")]
        public decimal? CostoDLL { get; set; }

        [Column("ventaMN", TypeName = "decimal(18,4)")]
        public decimal? VentaMN { get; set; }

        [Column("ventaDLL", TypeName = "decimal(18,4)")]
        public decimal? VentaDLL { get; set; }

        [Column("stockmin" )]
        public int? StockMin { get; set; }

        [Column("stockmax")]
        public int? StockMax { get; set; }

        [Column("picture")]
        [StringLength(500)]
        public string? Picture { get; set; }

        [Column("vigente")]
        public bool? Vigente { get; set; }

        [Column("descriptionpackage")]
        [StringLength(500)]
        public string? DescriptionPackage { get; set; }

        [Column("packagequantity", TypeName = "decimal(18,4)")]
        public decimal? PackageQuantity { get; set; }

        [Column("measure")]
        [StringLength(100)]
        public string? Measure { get; set; }

        [Column("weightorvolumes", TypeName = "decimal(18,4)")]
        public decimal? WeightOrVolumes { get; set; }

        [Column("expiration")]
        public int? Expiration { get; set; }

        [Column("folioOcorReq")]
        [StringLength(100)]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? FolioOcorReq { get; set; }

        [Column("fechaOc")]
        public DateTime? FechaOc { get; set; }

        [Column("inoroutquantity", TypeName = "decimal(18,4)")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public decimal? InOrOutQuantity { get; set; } = 0;

        [Column("pending", TypeName = "decimal(18,4)")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public decimal? Pending { get; set; } = 0;
    }
}