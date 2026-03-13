using Application.Common.Interfaces.Repositories;
using Application.Services.Commands;
using Application.Services.Exceptions;
using Domain.Entities;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;

namespace Application.UnitTests.Services.CommandHandlerTests;

public class CreateServiceCommandHandlerTests
{
    private readonly IServiceRepository _repository;
    private readonly CreateServiceCommandHandler _sut;

    public CreateServiceCommandHandlerTests()
    {
        _repository = Substitute.For<IServiceRepository>();
        _sut = new CreateServiceCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenValidData_CreatesAndReturnsSuccess()
    {
        // Arrange
        var command = new CreateServiceCommand
        {
            Name = "Therapy Session",
            DurationMinutes = 50,
            Price = 150.00m,
            Description = "Standard therapy session"
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _repository.Received(1).AddAsync(Arg.Any<Service>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ReturnsUnknownException()
    {
        // Arrange
        var command = new CreateServiceCommand
        {
            Name = "Therapy Session",
            DurationMinutes = 50,
            Price = 150.00m,
            Description = "Standard"
        };

        _repository.AddAsync(Arg.Any<Service>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("DB is down"));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeTrue();
        result.IfError(err => err.ShouldBeOfType<ServiceUnknownException>());
    }
}