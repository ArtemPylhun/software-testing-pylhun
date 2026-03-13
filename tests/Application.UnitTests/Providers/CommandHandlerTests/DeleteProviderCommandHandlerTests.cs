using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Application.Providers.Commands;
using Application.Providers.Exceptions;
using Domain.Entities;
using Domain.ValueObjects;
using NSubstitute;
using Shouldly;

namespace Application.UnitTests.Providers.CommandHandlerTests;

public class DeleteProviderCommandHandlerTests
{
    private readonly IProviderRepository _repository;
    private readonly IProviderQueries _queries;
    private readonly DeleteProviderCommandHandler _sut;

    public DeleteProviderCommandHandlerTests()
    {
        _repository = Substitute.For<IProviderRepository>();
        _queries = Substitute.For<IProviderQueries>();
        _sut = new DeleteProviderCommandHandler(_repository, _queries);
    }

    private Provider CreateValidProvider(Guid id)
    {
        var provider = Provider.Create("Dr. House", "Diag", "h@h.com", new TimeOnly(9, 0), new TimeOnly(18, 0));
        var idProperty = typeof(Provider).GetProperty(nameof(Provider.Id));
        idProperty?.SetValue(provider, new ProviderId(id));
        return provider;
    }

    [Fact]
    public async Task Handle_WhenProviderExists_DeletesAndReturnsSuccess()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new DeleteProviderCommand { ProviderId = providerId };
        var provider = CreateValidProvider(providerId);

        _queries.GetByIdAsync(Arg.Any<ProviderId>(), Arg.Any<CancellationToken>())
            .Returns(Optional.OptionExtensions.Some(provider));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _repository.Received(1).DeleteAsync(provider.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProviderNotFound_ReturnsError()
    {
        // Arrange
        var command = new DeleteProviderCommand { ProviderId = Guid.NewGuid() };

        _queries.GetByIdAsync(Arg.Any<ProviderId>(), Arg.Any<CancellationToken>())
            .Returns(Optional.Option.None<Provider>());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsError.ShouldBeTrue();
        result.IfError(err => err.ShouldBeOfType<ProviderNotFoundException>());
        
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<ProviderId>(), Arg.Any<CancellationToken>());
    }
}