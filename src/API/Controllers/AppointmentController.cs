using API.DTOs;
using API.Modules.Errors;
using Application.Appointments.Commands;
using Application.Common.Interfaces.Queries;
using Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/appointments")]
[ApiController]
public class AppointmentsController(ISender sender, IProviderQueries providerQueries) : ControllerBase
{
    [HttpGet("{id:guid}/slots")]
    public async Task<ActionResult<List<TimeOnly>>> GetAvailableSlots([FromRoute] Guid id, [FromQuery] DateOnly date, CancellationToken cancellationToken)
    {
        var slots = await providerQueries.GetAvailableSlotsAsync(new ProviderId(id), date, cancellationToken);
        
        return Ok(slots);
    }
    
    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> Create([FromBody] AppointmentCreateDto request, CancellationToken cancellationToken)
    {
        var input = new CreateAppointmentCommand
        {
            ProviderId = request.ProviderId,
            ServiceId = request.ServiceId,
            ClientName = request.ClientName,
            ClientEmail = request.ClientEmail,
            Date = request.Date,
            StartTime = request.StartTime
        };

        var result = await sender.Send(input, cancellationToken);
        return result.Match<ActionResult<AppointmentDto>>(
            a => AppointmentDto.FromDomainModel(a), e => e.ToObjectResult());
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<ActionResult<AppointmentDto>> Cancel([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CancelAppointmentCommand { AppointmentId = id }, cancellationToken);
        return result.Match<ActionResult<AppointmentDto>>(
            a => AppointmentDto.FromDomainModel(a), e => e.ToObjectResult());
    }

    [HttpPatch("{id:guid}/complete")]
    public async Task<ActionResult<AppointmentDto>> Complete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CompleteAppointmentCommand { AppointmentId = id }, cancellationToken);
        return result.Match<ActionResult<AppointmentDto>>(
            a => AppointmentDto.FromDomainModel(a), e => e.ToObjectResult());
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<AppointmentDto>> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteAppointmentCommand { AppointmentId = id }, cancellationToken);
        return result.Match<ActionResult<AppointmentDto>>(a => AppointmentDto.FromDomainModel(a), e => e.ToObjectResult());
    }
    
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AppointmentDto>> Update([FromRoute] Guid id, [FromBody] AppointmentUpdateDto request, CancellationToken cancellationToken)
    {
        if (id != request.Id) return BadRequest("ID mismatch");
        var result = await sender.Send(new UpdateAppointmentCommand { /* мапінг */ AppointmentId = id, ProviderId = request.ProviderId, ServiceId = request.ServiceId, Date = request.Date, StartTime = request.StartTime }, cancellationToken);
        return result.Match<ActionResult<AppointmentDto>>(a => AppointmentDto.FromDomainModel(a), e => e.ToObjectResult());
    }
}