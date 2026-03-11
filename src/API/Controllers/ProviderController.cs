using API.DTOs;
using API.Modules.Errors;
using Application.Common.Interfaces.Queries;
using Application.Providers.Commands;
using Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/providers")]
[ApiController]
public class ProvidersController(ISender sender, IProviderQueries providerQueries, IAppointmentQueries appointmentQueries) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ProviderDto>>> GetAll(CancellationToken cancellationToken = default)
    {
        var providers = await providerQueries.GetAllAsync(cancellationToken);
        return Ok(providers.Select(ProviderDto.FromDomainModel).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<ProviderDto>> Create([FromBody] ProviderCreateDto request, CancellationToken cancellationToken)
    {
        var input = new CreateProviderCommand
        {
            Name = request.Name,
            Specialization = request.Specialization,
            Email = request.Email,
            StartWorkingHours = request.StartWorkingHours,
            EndWorkingHours = request.EndWorkingHours
        };

        var result = await sender.Send(input, cancellationToken);
        return result.Match<ActionResult<ProviderDto>>(
            p => ProviderDto.FromDomainModel(p), e => e.ToObjectResult());
    }

    [HttpGet("{id:guid}/slots")]
    public async Task<ActionResult<List<TimeSlotDto>>> GetAvailableSlots([FromRoute] Guid id, [FromQuery] DateOnly date, CancellationToken cancellationToken)
    {
        var slots = await providerQueries.GetAvailableSlotsAsync(new ProviderId(id), date, cancellationToken);
        return Ok(slots);
    }

    [HttpGet("{id:guid}/appointments")]
    public async Task<ActionResult<List<AppointmentDto>>> GetSchedule([FromRoute] Guid id, [FromQuery] DateOnly date, CancellationToken cancellationToken)
    {
        var appointments = await appointmentQueries.GetScheduleByProviderAsync(new ProviderId(id), date, cancellationToken);
        return Ok(appointments.Select(AppointmentDto.FromDomainModel).ToList());
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProviderDto>> Update([FromRoute] Guid id, [FromBody] ProviderUpdateDto request, CancellationToken cancellationToken)
    {
        if (id != request.Id) return BadRequest("ID mismatch");
        var result = await sender.Send(new UpdateProviderCommand { /* мапінг */ ProviderId = id, Name = request.Name, Specialization = request.Specialization, Email = request.Email, StartWorkingHours = request.StartWorkingHours, EndWorkingHours = request.EndWorkingHours }, cancellationToken);
        return result.Match<ActionResult<ProviderDto>>(p => ProviderDto.FromDomainModel(p), e => e.ToObjectResult());
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ProviderDto>> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteProviderCommand { ProviderId = id }, cancellationToken);
        return result.Match<ActionResult<ProviderDto>>(p => ProviderDto.FromDomainModel(p), e => e.ToObjectResult());
    }
}