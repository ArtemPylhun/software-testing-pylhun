using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Optional;

namespace Infrastructure.Persistence.Repositories;

public class ServiceRepository : IServiceRepository, IServiceQueries
{
    private readonly AppDbContext _context;

    public ServiceRepository(AppDbContext context) => _context = context;

    public async Task<List<Service>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Services
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<Option<Service>> GetByIdAsync(ServiceId id, CancellationToken ct = default)
    {
        var service = await _context.Services
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        return service == null ? Option.None<Service>() : Option.Some(service);
    }

    public async Task AddAsync(Service service, CancellationToken ct = default)
    {
        await _context.Services.AddAsync(service, ct);
        await _context.SaveChangesAsync(ct);
    }
    
    public async Task UpdateAsync(Service service, CancellationToken ct = default)
    {
        _context.Services.Update(service);
        await _context.SaveChangesAsync(ct);
    }
    
    public async Task DeleteAsync(ServiceId id, CancellationToken ct = default)
    {
        var service = await _context.Services.FindAsync(new object[] { id }, ct);
        
        if (service != null)
        {
            _context.Services.Remove(service);
            await _context.SaveChangesAsync(ct);
        }
    }
    
}