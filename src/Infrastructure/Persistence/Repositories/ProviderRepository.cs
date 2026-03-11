using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Optional;

namespace Infrastructure.Persistence.Repositories;

public class ProviderRepository : IProviderRepository, IProviderQueries
{
    private readonly AppDbContext _context;

    public ProviderRepository(AppDbContext context) => _context = context;

public async Task<List<Provider>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Providers
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<Option<Provider>> GetByIdAsync(ProviderId id, CancellationToken ct = default)
    {
        var provider = await _context.Providers
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        return provider == null ? Option.None<Provider>() : Option.Some(provider);
    }

    public async Task<List<TimeOnly>> GetAvailableSlotsAsync(ProviderId providerId, DateOnly date, CancellationToken ct = default)
    {
        var provider = await _context.Providers
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == providerId, ct);

        if (provider == null) return new List<TimeOnly>();

        var bookedAppointments = await _context.Appointments
            .Where(a => a.ProviderId == providerId && a.Date == date && a.Status != AppointmentStatus.Cancelled)
            .AsNoTracking()
            .ToListAsync(ct);

        var availableSlots = new List<TimeOnly>();
        var currentSlot = provider.StartWorkingHours;
        var slotDuration = TimeSpan.FromMinutes(30);

        while (currentSlot.Add(slotDuration) <= provider.EndWorkingHours)
        {
            var slotEnd = currentSlot.Add(slotDuration);
            
            bool isOverlapping = bookedAppointments.Any(a => 
                a.StartTime < slotEnd && a.EndTime > currentSlot);

            if (!isOverlapping)
            {
                availableSlots.Add(currentSlot);
            }

            currentSlot = currentSlot.Add(slotDuration);
        }

        return availableSlots;
    }

    public async Task AddAsync(Provider provider, CancellationToken ct = default)
    {
        await _context.Providers.AddAsync(provider, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Provider provider, CancellationToken ct = default)
    {
        _context.Providers.Update(provider);
        await _context.SaveChangesAsync(ct);
    }
    
    public async Task DeleteAsync(ProviderId id, CancellationToken ct = default)
    {
        var provider = await _context.Providers.FindAsync(new object[] { id }, ct);
        
        if (provider != null)
        {
            _context.Providers.Remove(provider);
            await _context.SaveChangesAsync(ct);
        }
    }
}