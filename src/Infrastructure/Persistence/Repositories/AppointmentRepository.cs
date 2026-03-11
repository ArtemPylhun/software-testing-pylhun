using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Optional;

namespace Infrastructure.Persistence.Repositories;

public class AppointmentRepository : IAppointmentRepository, IAppointmentQueries
{
    private readonly AppDbContext _context;

    public AppointmentRepository(AppDbContext context) => _context = context;

    public async Task<Option<Appointment>> GetByIdAsync(AppointmentId id, CancellationToken ct = default)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Provider)
            .Include(a => a.Service)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        return appointment == null ? Option.None<Appointment>() : Option.Some(appointment);
    }

    public async Task<List<Appointment>> GetScheduleByProviderAsync(ProviderId providerId, DateOnly date,
        CancellationToken ct = default)
    {
        return await _context.Appointments
            .Where(a => a.ProviderId == providerId && a.Date == date)
            .AsNoTracking()
            .OrderBy(a => a.StartTime)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Appointment appointment, CancellationToken ct = default)
    {
        await _context.Appointments.AddAsync(appointment, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Appointment appointment, CancellationToken ct = default)
    {
        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync(ct);
    }
    
    public async Task DeleteAsync(AppointmentId id, CancellationToken ct = default)
    {
        var appointment = await _context.Appointments.FindAsync(new object[] { id }, ct);
        
        if (appointment != null)
        {
            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync(ct);
        }
    }
}