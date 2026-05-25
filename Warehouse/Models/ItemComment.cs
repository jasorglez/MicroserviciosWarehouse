using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models;

[Table("item_comments", Schema = "Delison")]
public class ItemComment
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("document_type")]
    public string DocumentType { get; set; } = "";  // 'REQ', 'COTIZ', 'PROVEEDOR'

    [Column("id_document")]
    public int IdDocument { get; set; }

    [Column("num_article")]
    public string NumArticle { get; set; } = "";

    [Column("id_user")]
    public int IdUser { get; set; }

    [Column("user_name")]
    public string UserName { get; set; } = "";

    [Column("text")]
    public string Text { get; set; } = "";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("active")]
    public bool Active { get; set; } = true;

    [Column("id_provider")]
    public int? IdProvider { get; set; }
}
