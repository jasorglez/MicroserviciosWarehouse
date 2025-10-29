using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Warehouse.Models.Views
{
    public class ProveedoresxtypeView
    {
        public int Id { get; set; }

        [Column("namecontact")]
        public string? NameContact { get; set; }

        [Column("company")]
        public string? Company { get; set; }

        [Column("phone")]
        public string? Phone { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("cp")]
        public string? Cp { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [Column("state")]
        public string? State { get; set; }

        [Column("city")]
        public string? City { get; set; }

        [Column("fieldbank")]
        public int? FieldBank { get; set; }

        [Column("fieldcontact")]
        public int? FieldContact { get; set; }

        [Column("fieldcuenta")]
        public int? FieldCuenta { get; set; }

        [Column("fieldmaterial")]
        public int? FieldMaterial { get; set; }

        [Column("id_root")]
        public int IdRoot { get; set; }

        [Column("typework")]
        public string? Typework { get; set; }

        [Column("vigente")]
        public bool? Vigente { get; set; }

    }
}






