using Easy.Mediator.UnitTests.Utils.Pipelines;
using Easy.Mediator.UnitTests.Utils.Requests;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Easy.Mediator.UnitTests;

public class MediatorPipelineFluentValidationTests
{
    private readonly IMediator _mediator;

    public MediatorPipelineFluentValidationTests()
    {
        var services = new ServiceCollection();

        services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

        services.AddEasyMediator(config =>
        {
            config.AddPipelineBehavior(typeof(FluentValidationBehavior<,>));
        });

        var provider = services.BuildServiceProvider();

        _mediator = provider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task Send_Should_Throw_When_Name_Is_Empty()
    {
        var user = new UserCreateCommand("");

        var exception = await Assert.ThrowsAsync<ValidationException>(() => _mediator.Send(user));

        var errors = JsonSerializer.Serialize(exception.Errors);

        Assert.Contains("Name is required", errors);
    }

    [Fact]
    public async Task Send_Should_Throw_When_Name_Is_Null()
    {
        var user = new UserCreateCommand(null!);

        var exception = await Assert.ThrowsAsync<ValidationException>(() => _mediator.Send(user));

        var errors = JsonSerializer.Serialize(exception.Errors);

        Assert.Contains("Name is required", errors);
    }

    [Fact]
    public async Task Send_Should_Throw_When_Name_Is_Too_Short()
    {
        var user = new UserCreateCommand("Jo");

        var exception = await Assert.ThrowsAsync<ValidationException>(() => _mediator.Send(user));

        var errors = JsonSerializer.Serialize(exception.Errors);

        Assert.Contains("Name must be at least 3 characters", errors);
    }

    [Fact]
    public async Task Send_Should_Not_Throw_When_Name_Is_Valid()
    {
        var user = new UserCreateCommand("John Doe");

        var result = await _mediator.Send(user);

        Assert.NotNull(result);
    }
}