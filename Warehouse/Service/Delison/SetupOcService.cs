using Microsoft.EntityFrameworkCore;
using Warehouse.Models.Delison;

namespace Warehouse.Service.Delison
{
    public interface ISetupOcService
    {
        Task<SetupOc?> GetByCompany(int idCompany);
        Task<SetupOc?> GetByBranch(int idBranch);
        Task<SetupOc> CreateOrUpdate(int idCompany, SetupOc data);
        Task<SetupOc> CreateOrUpdateByBranch(int idBranch, SetupOc data);
    }

    public class SetupOcService : ISetupOcService
    {
        private readonly DbWarehouseContext _context;

        public SetupOcService(DbWarehouseContext context)
        {
            _context = context;
        }

        public async Task<SetupOc?> GetByCompany(int idCompany)
        {
            return await _context.SetupOc
                .Where(s => s.IdCompany == idCompany && s.Active)
                .FirstOrDefaultAsync();
        }

        public async Task<SetupOc?> GetByBranch(int idBranch)
        {
            return await _context.SetupOc
                .Where(s => s.IdBranch == idBranch && s.Active)
                .FirstOrDefaultAsync();
        }

        public async Task<SetupOc> CreateOrUpdate(int idCompany, SetupOc data)
        {
            var existing = await _context.SetupOc
                .Where(s => s.IdCompany == idCompany && s.Active)
                .FirstOrDefaultAsync();

            if (existing == null)
            {
                data.IdCompany   = idCompany;
                data.EntregaMin  = 1;
                data.DateModified = DateTime.Now;
                _context.SetupOc.Add(data);
            }
            else
            {
                existing.EntregaMax   = data.EntregaMax;
                existing.DateModified = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return existing ?? data;
        }

        public async Task<SetupOc> CreateOrUpdateByBranch(int idBranch, SetupOc data)
        {
            var existing = await _context.SetupOc
                .Where(s => s.IdBranch == idBranch && s.Active)
                .FirstOrDefaultAsync();

            if (existing == null)
            {
                data.IdBranch    = idBranch;
                data.EntregaMin  = 1;
                data.DateModified = DateTime.Now;
                _context.SetupOc.Add(data);
            }
            else
            {
                existing.EntregaMax   = data.EntregaMax;
                existing.DateModified = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return existing ?? data;
        }
    }
}
