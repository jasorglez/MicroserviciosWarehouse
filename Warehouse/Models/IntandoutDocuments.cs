using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Warehouse.Models
{
    [Table("intandout_documents")]
    public class IntandoutDocuments
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_doc")]
        public int? IdDoc { get; set; }

        [Column("document_name")]
        public string? DocumentName { get; set; }

        [Column("url_document")]
        public string? UrlDocument { get; set; }

        [Column("type")]
        public string? Type { get; set; }
    }
}