using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Providers.Commands;
using Application.Providers.Exceptions;
using Domain.Entities;
using Domain.ValueObjects;
using NSubstitute;
using Shouldly;

namespace Application.UnitTests.Providers.CommandHandlerTests;

public class UpdateProviderCommandHandlerTests
{
    private readonly IProviderRepository _repository;
    private readonly IProviderQueries _queries;
    private readonly UpdateProviderCommandHandler _sut;

    public UpdateProviderCommandHandlerTests()
    {
        _repository = Substitute.For<IProviderRepository>();
        _queries = Substitute.For<IProviderQueries>();
        _sut = new UpdateProviderCommandHandler(_repository, _queries);
    }

    private Provider CreateValidProvider(Guid id)
    {
        var provider = Provider.Create("Old Name", "Old Spec", "old@test.com", new TimeOnly(8, 0), new TimeOnly(17, 0));
        var idProperty = typeof(Provider).GetProperty(nameof(Provider.Id));
        idProperty?.SetValue(provider, new ProviderId(id));
        return provider;
    }

    [Fact]
    public async Task Handle_WhenProviderExists_UpdatesAndReturnsSuccess()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new UpdateProviderCommand
        {
            ProviderId = providerId,
            Name = "New Name",
            Specialization = "New Spec",
            Email = "new@test.com",
            StartWorkingHours = new TimeOnly(10, 0),
            EndWorkingHours = new TimeOnly(19, 0)
        };
        var provider = CreateValidProvider(providerId);

        _queries.GetByIdAsync(Arg.Any<ProviderId>(), Arg.Any<CancellationToken>())
            .Returns(Optional.OptionExtensions.Some(provider));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _repository.Received(1).UpdateAsync(provider, Arg.Any<CancellationToken>());
        
        // Переконуємось, що властивості дійсно оновилися перед тим, як потрапити в репозиторій
        provider.Name.ShouldBe(command.Name);
    }

    [Fact]
    public async Task Handle_WhenProviderNotFound_ReturnsError()
    {
        // Arrange
        var command = new UpdateProviderCommand { ProviderId = Guid.NewGuid() };

        _queries.GetByIdAsync(Arg.Any<ProviderId>(), Arg.Any<CancellationToken>())
            .Returns(Optional.Option.None<Provider>());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeTrue();
        result.IfError(err => err.ShouldBeOfType<ProviderNotFoundException>());
        
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Provider>(), Arg.Any<CancellationToken>());
    }
}