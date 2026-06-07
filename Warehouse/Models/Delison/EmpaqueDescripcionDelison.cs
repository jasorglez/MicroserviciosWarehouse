using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Models.Delison
{
    [Table("empaque_descripcion", Schema = "Delison")]
    public class EmpaqueDescripcionDelison
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // FK -> warehouses.dbo.proveedorxtablas.Id (par material-proveedor)
        [Column("id_proveedor_tabla")]
        public int IdProveedorTabla { get; set; }

        // FK -> Delison.descripcion_empaque.id
        [Column("id_descripcion_empaque")]
        public int? IdDescripcionEmpaque { get; set; }

        [Column("pieza_x_paquete")]
        public int? PiezaXPaquete { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;

        [Column("datemodified")]
        public DateTime DateModified { get; set; } = DateTime.Now;
    }
}
