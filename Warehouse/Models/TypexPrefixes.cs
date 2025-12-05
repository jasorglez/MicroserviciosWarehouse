using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models;

[Table("typexprefixes")]
public class TypexPrefixes
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("req_type")]
    public string ReqType { get; set; }
    
    [Column("id_req_type")]
    public int IdReqType { get; set; }
    
    [Column("prefix")]
    public string?  Prefix { get; set; }
    
    [Column("consecutive")]
    public int Consecutive { get; set; }
    
    [Column("active")]
    public bool Active { get; set; }
}