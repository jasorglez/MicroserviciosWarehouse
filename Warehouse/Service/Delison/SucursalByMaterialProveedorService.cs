using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Warehouse.Models.Views;
using Warehouse.Models;
using Warehouse.Models.Delison;

 
namespace Warehouse.Service.Delison
{
    public class SucursalByMaterialProveedorService : ISucursalByMaterialProveedorService
    {
        private readonly DbWarehouseContext _context;
        private readonly ILogger<SucursalByMaterialProveedorService> _logger;

        public SucursalByMaterialProveedorService(DbWarehouseContext context, ILogger<SucursalByMaterialProveedorService> logger)
        {
            _context = context;
            _logger = logger;
        }


        public async Task<List<SucursalByMaterialProveedor>> GetAll( int idMaster)
        {
            try
            { 
                // Obtener los IDs de master families existentes para la compañía
                /*var existentes = await _context.SucursalByMaterialProveedor
                    .Where(m => m.IdCompany == idCompany && m.Active == true && m.MasterFamily != null)
                    .Select(m => m.MasterFamily.Value)
                    .ToListAsync();
                    && !existentes.Contains(c.Id)*/

                // Consultar catálogos tipo FAM-CAT que no estén ya registrados en SucursalByMaterialProveedor
                var items = await _context.SucursalByMaterialProveedor
                    .Where(c => c.Active == true && c.IdMaterialByProveedor == idMaster)
                    .OrderByDescending(c => c.Vigente)
                    .ToListAsync();

                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Catalogs");
                throw;
            }
        }

        public async Task<bool> SaveMasterDetail(SucursalByMaterialProveedor detail)
        {
            var data = new SucursalByMaterialProveedor
            {
                IdMaterialByProveedor = detail.IdMaterialByProveedor,
                IdSucursal = detail.IdSucursal,
                FechaAlta = detail.FechaAlta,
                StockMinimo = detail.StockMinimo,
                Resurtido = detail.Resurtido,
                CapacidadMaxAlmacen = detail.CapacidadMaxAlmacen,
                TiempoDeEntrega = detail.TiempoDeEntrega,
                Vigente = detail.Vigente,
                Active = true
            };
        
            _context.SucursalByMaterialProveedor.Add(data);
        
            var saved = await _context.SaveChangesAsync();
        
            return saved > 0;
        }

        public async Task<SucursalByMaterialProveedor> UpdateMasterDetail(int id, SucursalByMaterialProveedor detail)
        {
            var existingItem = await _context.SucursalByMaterialProveedor.FindAsync(id);
            if (existingItem == null)
            {
                _logger.LogWarning("Attempted to update non-existent Supply with ID {Id}", id);
                return null;
            }

            try
            {
                existingItem.IdMaterialByProveedor = detail.IdMaterialByProveedor;
                existingItem.IdSucursal = detail.IdSucursal;
                existingItem.FechaAlta = detail.FechaAlta;
                existingItem.StockMinimo = detail.StockMinimo;
                existingItem.Resurtido = detail.Resurtido;
                existingItem.CapacidadMaxAlmacen = detail.CapacidadMaxAlmacen;
                existingItem.TiempoDeEntrega = detail.TiempoDeEntrega;
                existingItem.Vigente = detail.Vigente;
                await _context.SaveChangesAsync();

            return existingItem;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating Supply with ID {Id}", id);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Supply with ID {Id}", id);
                throw;
            }


        }
      
        public async Task<bool> Delete(int id)
        {
            var existingItem = await _context.SucursalByMaterialProveedor.FindAsync(id);
            if (existingItem == null)
            {
                _logger.LogWarning("Attempted to delete non-existent Supply with ID {Id}", id);
                return false;
            }

            try
            {
                existingItem.Active = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Supply with ID {Id}", id);
                throw;
            }
        }

    }
    public interface ISucursalByMaterialProveedorService
    {
        Task<List<SucursalByMaterialProveedor>> GetAll( int idMaster);
        Task<bool> SaveMasterDetail(SucursalByMaterialProveedor detail);
        Task<bool> Delete(int id);
        Task<SucursalByMaterialProveedor> UpdateMasterDetail(int id, SucursalByMaterialProveedor detail);
    }

// ...existing code...
}
