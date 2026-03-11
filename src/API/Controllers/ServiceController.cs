using API.DTOs;
using API.Modules.Errors;
using Application.Common.Interfaces.Queries;
using Application.Services.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/services")]
[ApiController]
public class ServicesController(ISender sender, IServiceQueries serviceQueries) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ServiceDto>>> GetAll(CancellationToken cancellationToken = default)
    {
        var services = await serviceQueries.GetAllAsync(cancellationToken);
        return Ok(services.Select(ServiceDto.FromDomainModel).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<ServiceDto>> Create([FromBody] ServiceCreateDto request, CancellationToken cancellationToken)
    {
        var input = new CreateServiceCommand
        {
            Name = request.Name,
            DurationMinutes = request.DurationMinutes,
            Price = request.Price,
            Description = request.Description
        };

        var result = await sender.Send(input, cancellationToken);
        return result.Match<ActionResult<ServiceDto>>(
            s => ServiceDto.FromDomainModel(s), e => e.ToObjectResult());
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ServiceDto>> Update([FromRoute] Guid id, [FromBody] ServiceUpdateDto request, CancellationToken cancellationToken)
    {
        if (id != request.Id) return BadRequest("ID mismatch");
        var result = await sender.Send(new UpdateServiceCommand { /* мапінг */ ServiceId = id, Name = request.Name, DurationMinutes = request.DurationMinutes, Price = request.Price, Description = request.Description }, cancellationToken);
        return result.Match<ActionResult<ServiceDto>>(s => ServiceDto.FromDomainModel(s), e => e.ToObjectResult());
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ServiceDto>> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteServiceCommand { ServiceId = id }, cancellationToken);
        return result.Match<ActionResult<ServiceDto>>(s => ServiceDto.FromDomainModel(s), e => e.ToObjectResult());
    }
}