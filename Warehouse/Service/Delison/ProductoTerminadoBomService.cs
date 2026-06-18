using Microsoft.EntityFrameworkCore;
using Warehouse.Models.Delison;

namespace Warehouse.Service.Delison
{
    public interface IProductoTerminadoBomService
    {
        Task<List<ProductoTerminadoBomDelison>> GetByCompany(int idCompany);
        Task<List<ProductoTerminadoBomDelison>> GetByRoot(int idCompany, int idProductoRoot);
        Task<ProductoTerminadoBomDelison?> GetById(int id);
        Task<ProductoTerminadoBomDelison> Create(ProductoTerminadoBomDelison data);
        Task<ProductoTerminadoBomDelison?> Update(int id, ProductoTerminadoBomDelison data);
        Task<bool> Delete(int id);
    }

    public class ProductoTerminadoBomService : IProductoTerminadoBomService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<ProductoTerminadoBomService> _logger;

        public ProductoTerminadoBomService(DbWarehouseContext context, ILogger<ProductoTerminadoBomService> logger)
        {
            _context = context;
            _logger  = logger;
        }

        public async Task<List<ProductoTerminadoBomDelison>> GetByCompany(int idCompany)
        {
            return await _context.ProductoTerminadoBom
                .Where(b => b.IdCompany == idCompany && b.Active)
                .OrderBy(b => b.IdProductoRoot).ThenBy(b => b.Nivel).ThenBy(b => b.Orden)
                .AsNoTracking()
                .ToListAsync();
        }

        // Carga el arbol completo (todos los niveles) de un producto terminado.
        public async Task<List<ProductoTerminadoBomDelison>> GetByRoot(int idCompany, int idProductoRoot)
        {
            return await _context.ProductoTerminadoBom
                .Where(b => b.IdCompany == idCompany && b.IdProductoRoot == idProductoRoot && b.Active)
                .OrderBy(b => b.Nivel).ThenBy(b => b.Orden)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ProductoTerminadoBomDelison?> GetById(int id)
        {
            return await _context.ProductoTerminadoBom
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<ProductoTerminadoBomDelison> Create(ProductoTerminadoBomDelison data)
        {
            data.Active       = true;
            data.DateModified = DateTime.UtcNow;
            _context.ProductoTerminadoBom.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<ProductoTerminadoBomDelison?> Update(int id, ProductoTerminadoBomDelison data)
        {
            var existing = await _context.ProductoTerminadoBom.FindAsync(id);
            if (existing == null) return null;

            existing.IdProductoRoot = data.IdProductoRoot;
            existing.IdPadre        = data.IdPadre;
            existing.IdMaterial     = data.IdMaterial;
            existing.Nombre         = data.Nombre;
            existing.EsBasico       = data.EsBasico;
            existing.Cantidad       = data.Cantidad;
            existing.Unidad         = data.Unidad;
            existing.MermaPct       = data.MermaPct;
            existing.CostoUnitario  = data.CostoUnitario;
            existing.CostoTotal     = data.CostoTotal;
            existing.Orden          = data.Orden;
            existing.Nivel          = data.Nivel;
            existing.Comentarios    = data.Comentarios;
            existing.ModoCosto      = data.ModoCosto;
            existing.Active         = data.Active;
            existing.DateModified   = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        // Soft delete en cascada: borra el nodo y todos sus descendientes.
        public async Task<bool> Delete(int id)
        {
            var existing = await _context.ProductoTerminadoBom.FindAsync(id);
            if (existing == null) return false;

            // Recolectar todos los descendientes (BFS) dentro del mismo arbol.
            var arbol = await _context.ProductoTerminadoBom
                .Where(b => b.IdProductoRoot == existing.IdProductoRoot && b.Active)
                .ToListAsync();

            var aBorrar = new HashSet<int> { existing.Id };
            bool agregado;
            do
            {
                agregado = false;
                foreach (var n in arbol)
                {
                    if (n.IdPadre.HasValue && aBorrar.Contains(n.IdPadre.Value) && !aBorrar.Contains(n.Id))
                    {
                        aBorrar.Add(n.Id);
                        agregado = true;
                    }
                }
            } while (agregado);

            foreach (var n in arbol.Where(n => aBorrar.Contains(n.Id)))
            {
                n.Active       = false;
                n.DateModified = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
