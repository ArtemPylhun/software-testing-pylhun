using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Services.Commands;
using Application.Services.Exceptions;
using Domain.Entities;
using Domain.ValueObjects;
using NSubstitute;
using Shouldly;

namespace Application.UnitTests.Services.CommandHandlerTests;

public class UpdateServiceCommandHandlerTests
{
    private readonly IServiceRepository _repository;
    private readonly IServiceQueries _queries;
    private readonly UpdateServiceCommandHandler _sut;

    public UpdateServiceCommandHandlerTests()
    {
        _repository = Substitute.For<IServiceRepository>();
        _queries = Substitute.For<IServiceQueries>();
        _sut = new UpdateServiceCommandHandler(_repository, _queries);
    }

    private Service CreateValidService(Guid id)
    {
        var service = Service.Create("Old Service", 30, 50m, "Old Desc");
        var idProperty = typeof(Service).GetProperty(nameof(Service.Id));
        idProperty?.SetValue(service, new ServiceId(id));
        return service;
    }

    [Fact]
    public async Task Handle_WhenServiceExists_UpdatesAndReturnsSuccess()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var command = new UpdateServiceCommand
        {
            ServiceId = serviceId,
            Name = "Updated Service",
            DurationMinutes = 90,
            Price = 200m,
            Description = "Updated Desc"
        };
        var service = CreateValidService(serviceId);

        _queries.GetByIdAsync(Arg.Any<ServiceId>(), Arg.Any<CancellationToken>())
            .Returns(Optional.OptionExtensions.Some(service));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _repository.Received(1).UpdateAsync(service, Arg.Any<CancellationToken>());
        
        // Перевіряємо, що властивості оновилися перед збереженням
        service.Name.ShouldBe(command.Name);
        service.DurationMinutes.ShouldBe(command.DurationMinutes);
        service.Price.ShouldBe(command.Price);
    }

    [Fact]
    public async Task Handle_WhenServiceNotFound_ReturnsError()
    {
        // Arrange
        var command = new UpdateServiceCommand { ServiceId = Guid.NewGuid() };

        _queries.GetByIdAsync(Arg.Any<ServiceId>(), Arg.Any<CancellationToken>())
            .Returns(Optional.Option.None<Service>());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeTrue();
        result.IfError(err => err.ShouldBeOfType<ServiceNotFoundException>());
        
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Service>(), Arg.Any<CancellationToken>());
    }
}