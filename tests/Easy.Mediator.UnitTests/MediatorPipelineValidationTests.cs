using Easy.Mediator.UnitTests.Utils.Pipelines;
using Easy.Mediator.UnitTests.Utils.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace Easy.Mediator.UnitTests;

public class MediatorPipelineValidationTests
{
    private readonly IMediator _mediator;

    public MediatorPipelineValidationTests()
    {
        var services = new ServiceCollection();

        services.AddEasyMediator(config =>
        {
            config.AddPipelineBehavior(typeof(ValidationBehavior<,>));
        });

        var provider = services.BuildServiceProvider();

        _mediator = provider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task Send_Should_Throw_When_Name_Is_Missing_And_Not_Invoke_Handler()
    {
        // Arrange
        UserCreateHandler.WasCalled = false;

        var invalidCommand = new UserCreateCommand("");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _mediator.Send(invalidCommand));

        Assert.Equal("Name is required", ex.Message);
        Assert.False(UserCreateHandler.WasCalled);
    }

    [Fact]
    public async Task Send_Should_Invoke_Handler_When_Name_Is_Valid()
    {
        // Arrange
        UserCreateHandler.WasCalled = false;

        var validCommand = new UserCreateCommand("Valid Name");

        // Act
        await _mediator.Send(validCommand);

        // Assert
        Assert.True(UserCreateHandler.WasCalled);
    }

    [Fact]
    public async Task Send_Should_Throw_When_Command_Is_Null()
    {
        // Arrange
        UserCreateHandler.WasCalled = false;

        UserCreateCommand nullCommand = null;

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NullReferenceException>(() => _mediator.Send(nullCommand));

        Assert.Equal(typeof(NullReferenceException), ex.GetType());
        Assert.False(UserCreateHandler.WasCalled);
    }
}
