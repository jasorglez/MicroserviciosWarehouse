
using Microsoft.EntityFrameworkCore;
using Warehouse.Models;
using System.Threading.Tasks;

namespace Warehouse.Service
{
   

    public class ProveedorXTablaService : IProveedorXTablaService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<ProveedorXTablaService> _logger;

        public ProveedorXTablaService(DbWarehouseContext context, ILogger<ProveedorXTablaService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<ProveedorXTabla>> GetAll(int idProveedor, string Type)
        {
            return await _context.ProveedorXTablas
                .Where(p => p.IdTabla == idProveedor && p.Type == Type && p.Campo6 != "0" && p.Campo6 != "0.00" &&p.Active)
                .OrderByDescending(p => p.Vigente)
                .ThenBy(p => p.Campo2.Trim().ToLower())
                .ToListAsync();
        }

        public async Task<List<ProveedorXTabla>> GetProvxMat(int idMaterial, string Type)
        {
            return await _context.ProveedorXTablas
                .Where(p => p.Campo1 == idMaterial && p.Type == Type && p.Active)
                .OrderBy(p => p.Campo2)
                .ToListAsync();
        }

        public async Task<List<ProveedorXTabla>> GetMatXSubfam(int idMaterial,int idFam, string Type)
        {
            return await _context.ProveedorXTablas
                .Where(m => m.Campo1 == idMaterial && m.IdTabla==idFam && m.Type == Type && m.Active)
                .OrderBy(m => m.Campo2)
                .ToListAsync();
        }

        public async Task<ProveedorXTabla?> GetById(int id)
        {
            return await _context.ProveedorXTablas
                .FirstOrDefaultAsync(p => p.Id == id && p.Active);
        }

        public async Task<List<ProveedorXTabla>> GetByType(string type)
        {
            return await _context.ProveedorXTablas
                .Where(p => p.Type == type && p.Active)
                .ToListAsync();
        }

        public async Task<ProveedorXTabla> Create(ProveedorXTabla proveedor)
        {
            _context.ProveedorXTablas.Add(proveedor);
            await _context.SaveChangesAsync();
            return proveedor;
        }

        public async Task<ProveedorXTabla?> Update(int id, ProveedorXTabla proveedor)
        {
            var existing = await _context.ProveedorXTablas.FindAsync(id);
            if (existing == null) return null;

            // Actualizar propiedades
            existing.IdTabla = proveedor.IdTabla;
            existing.Campo1 = proveedor.Campo1;
            existing.Campo2 = proveedor.Campo2;
            existing.Campo3 = proveedor.Campo3;
            existing.Campo4 = proveedor.Campo4;
            existing.Campo5 = proveedor.Campo5;
            existing.Campo6 = proveedor.Campo6;
            existing.Campo7 = proveedor.Campo7;
            existing.Campo8 = proveedor.Campo8;
            //existing.Type = proveedor.Type;
            existing.Vigente = proveedor.Vigente;
            existing.Principal = proveedor.Principal;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> Delete(int id)
        {
            var proveedor = await _context.ProveedorXTablas.FindAsync(id);
            if (proveedor == null) return false;

            _context.ProveedorXTablas.Remove(proveedor);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleActive(int id)
        {
            var proveedor = await _context.ProveedorXTablas.FindAsync(id);
            if (proveedor == null) return false;

            proveedor.Active = !proveedor.Active;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ProveedorXTabla?> UpdateAbonoTabla(int id, int table)
        {
            var existing = await _context.ProveedorXTablas.FindAsync(table);
            if (existing == null)
                return null;

            // Sumar todos los abonos activos para la tabla específica.
            // Usamos SumAsync para eficiencia y '?? 0' para manejar totales nulos.
            var totalAbono = await _context.CreditProveedores
                .Where(a => a.IdCustomer == id && a.IdProveedorXTablas == table && a.Active == true)
                .SumAsync(a => a.Total ?? 0);

            // Convertir Campo4 (monto original) a decimal para poder operar.
            if (!decimal.TryParse(existing.Campo4, out decimal montoOriginal))
            {
                // Opcional: Loggear un error si Campo4 no es un número válido.
                _logger.LogWarning("El valor de Campo4 '{Campo4Value}' no es un decimal válido para ProveedorXTabla con ID {TableId}", existing.Campo4, table);
                montoOriginal = 0; // Asumir 0 si la conversión falla.
            }

            decimal ultimoMonto = montoOriginal - totalAbono;
            
            existing.Campo5 = totalAbono.ToString();
            existing.Campo6 = ultimoMonto.ToString();

            await _context.SaveChangesAsync();

            return existing;
        }


    }


    public interface IProveedorXTablaService
    {
        Task<List<ProveedorXTabla>> GetAll(int idProveedor, string Type);
        Task<List<ProveedorXTabla>> GetProvxMat(int idMaterial, string Type);
        Task<List<ProveedorXTabla>> GetMatXSubfam(int idMaterial, int idFam, string Type);
        Task<ProveedorXTabla?> GetById(int id);
        Task<List<ProveedorXTabla>> GetByType(string type);
        Task<ProveedorXTabla> Create(ProveedorXTabla proveedor);
        Task<ProveedorXTabla?> Update(int id, ProveedorXTabla proveedor);
        Task<ProveedorXTabla?> UpdateAbonoTabla(int id, int table);
        Task<bool> Delete(int id);
        Task<bool> ToggleActive(int id);
    }

}