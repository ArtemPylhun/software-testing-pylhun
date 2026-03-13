using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Services.Commands;
using Application.Services.Exceptions;
using Domain.Entities;
using Domain.ValueObjects;
using NSubstitute;
using Shouldly;

namespace Application.UnitTests.Services.CommandHandlerTests;

public class DeleteServiceCommandHandlerTests
{
    private readonly IServiceRepository _repository;
    private readonly IServiceQueries _queries;
    private readonly DeleteServiceCommandHandler _sut;

    public DeleteServiceCommandHandlerTests()
    {
        _repository = Substitute.For<IServiceRepository>();
        _queries = Substitute.For<IServiceQueries>();
        _sut = new DeleteServiceCommandHandler(_repository, _queries);
    }

    private Service CreateValidService(Guid id)
    {
        var service = Service.Create("Test Service", 60, 100m, "Desc");
        var idProperty = typeof(Service).GetProperty(nameof(Service.Id));
        idProperty?.SetValue(service, new ServiceId(id));
        return service;
    }

    [Fact]
    public async Task Handle_WhenServiceExists_DeletesAndReturnsSuccess()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var command = new DeleteServiceCommand { ServiceId = serviceId };
        var service = CreateValidService(serviceId);

        _queries.GetByIdAsync(Arg.Any<ServiceId>(), Arg.Any<CancellationToken>())
            .Returns(Optional.OptionExtensions.Some(service));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _repository.Received(1).DeleteAsync(service.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenServiceNotFound_ReturnsError()
    {
        // Arrange
        var command = new DeleteServiceCommand { ServiceId = Guid.NewGuid() };

        _queries.GetByIdAsync(Arg.Any<ServiceId>(), Arg.Any<CancellationToken>())
            .Returns(Optional.Option.None<Service>());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeTrue();
        result.IfError(err => err.ShouldBeOfType<ServiceNotFoundException>());
        
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<ServiceId>(), Arg.Any<CancellationToken>());
    }
}