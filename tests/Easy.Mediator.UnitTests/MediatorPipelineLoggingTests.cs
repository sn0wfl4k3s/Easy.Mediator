using Easy.Mediator.UnitTests.Utils.Pipelines;
using Easy.Mediator.UnitTests.Utils.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace Easy.Mediator.UnitTests;

public class MediatorPipelineLoggingTests
{
    private readonly IMediator _mediator;

    public MediatorPipelineLoggingTests()
    {
        var services = new ServiceCollection();

        services.AddEasyMediator(config =>
        {
            config.AddPipelineBehavior(typeof(LoggingBehavior<,>));
        });

        var provider = services.BuildServiceProvider();

        _mediator = provider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task Send_ShouldHandlePipelineBehavior()
    {
        var command = new PingCommand("test");
        var response = await _mediator.Send(command);
        Assert.NotNull(response);
        Assert.Equal("test => Pong!", response.Message);
        Assert.True(LoggingBehavior<PingCommand, PongResponse>.Executed);
    }

    [Fact]
    public void Executed_Should_Be_False_Before_Any_Execution()
    {
        Assert.False(LoggingBehavior<PingCommand, PongResponse>.Executed);
    }

    [Fact]
    public async Task Send_Should_Set_Executed_True_After_Handler()
    {
        LoggingBehavior<PingCommand, PongResponse>.Executed = false;
        var command = new PingCommand("executed");
        await _mediator.Send(command);

        Assert.True(LoggingBehavior<PingCommand, PongResponse>.Executed);
    }

    [Fact]
    public async Task Send_Should_Reset_Executed_Between_Executions()
    {
        LoggingBehavior<PingCommand, PongResponse>.Executed = false;

        var command = new PingCommand("first");
        Assert.False(LoggingBehavior<PingCommand, PongResponse>.Executed);
        await _mediator.Send(command);
        Assert.True(LoggingBehavior<PingCommand, PongResponse>.Executed);

        LoggingBehavior<PingCommand, PongResponse>.Executed = false;

        var command2 = new PingCommand("second");
        Assert.False(LoggingBehavior<PingCommand, PongResponse>.Executed);
        await _mediator.Send(command2);
        Assert.True(LoggingBehavior<PingCommand, PongResponse>.Executed);
    }

    [Fact]
    public async Task Send_Should_Not_Throw_And_Executed_Should_Be_True()
    {
        LoggingBehavior<PingCommand, PongResponse>.Executed = false;
        var command = new PingCommand("safe");
        var response = await _mediator.Send(command);

        Assert.NotNull(response);
        Assert.True(LoggingBehavior<PingCommand, PongResponse>.Executed);
    }
}
