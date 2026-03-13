using Application.Common.Interfaces.Repositories;
using Application.Providers.Commands;
using Application.Providers.Exceptions;
using Domain.Entities;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;

namespace Application.UnitTests.Providers.CommandHandlerTests;

public class CreateProviderCommandHandlerTests
{
    private readonly IProviderRepository _repository;
    private readonly CreateProviderCommandHandler _sut;

    public CreateProviderCommandHandlerTests()
    {
        _repository = Substitute.For<IProviderRepository>();
        _sut = new CreateProviderCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenValidData_CreatesAndReturnsSuccess()
    {
        // Arrange
        var command = new CreateProviderCommand
        {
            Name = "Dr. House",
            Specialization = "Diagnostics",
            Email = "house@hospital.com",
            StartWorkingHours = new TimeOnly(9, 0),
            EndWorkingHours = new TimeOnly(18, 0)
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _repository.Received(1).AddAsync(Arg.Any<Provider>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ReturnsUnknownException()
    {
        // Arrange
        var command = new CreateProviderCommand
        {
            Name = "Dr. House",
            Specialization = "Diagnostics",
            Email = "house@hospital.com",
            StartWorkingHours = new TimeOnly(9, 0),
            EndWorkingHours = new TimeOnly(18, 0)
        };

        _repository.AddAsync(Arg.Any<Provider>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("Database explosion"));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeTrue();
        result.IfError(err => err.ShouldBeOfType<ProviderUnknownException>());
    }
}