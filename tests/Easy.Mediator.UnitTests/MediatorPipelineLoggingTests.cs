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
}
