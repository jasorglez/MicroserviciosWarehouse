 
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Warehouse.Models.Views
{
    
    /// Modelo para la vista materialsxproviders
        [Table("materialsxproviders", Schema = "dbo")]
    public class MaterialsXProvider
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        [StringLength(100)]
        public string Name { get; set; }

        [Column("folio")]
        [StringLength(50)]
        public string Folio { get; set; }

        [Column("datecreate")]
        public DateTime DateCreate { get; set; }

        [Column("id_supplie")]
        public int IdSupplie { get; set; }

        [Column("description")]
        [StringLength(255)]
        public string Description { get; set; }

        [Column("quantity")]
        public decimal Quantity { get; set; }

        [Column("price")]
        public decimal Price { get; set; }

        [Column("id_provider")]
        public int IdProvider { get; set; }

        [Column("nameproveedor")]
        [StringLength(200)]
        public string NombreProveedor { get; set; }

        [Column("descriptionpackage")]
        [StringLength(255)]
        public string DescriptionPackage { get; set; }

        [Column("packagequantity")]
        public decimal? PackageQuantity { get; set; }

        [Column("measure")]
        [StringLength(50)]
        public string Measure { get; set; }

        [Column("weightorvolumes")]
        public decimal? WeightOrVolumes { get; set; }

        [Column("expiration")]
        public int? Expiration { get; set; }

        [Column("id_reference")]
        public int IdReference { get; set; }

        [Column("type_reference")]
        [StringLength(20)]
        public string TypeReference { get; set; }
    

    }
}