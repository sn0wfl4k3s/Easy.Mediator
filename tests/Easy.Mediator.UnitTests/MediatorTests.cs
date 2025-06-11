using Microsoft.Extensions.DependencyInjection;

namespace Easy.Mediator.UnitTests;

public class MediatorTests
{
    private readonly IMediator _mediator;

    public MediatorTests()
    {
        var services = new ServiceCollection();

        services.AddEasyMediator();

        var provider = services.BuildServiceProvider();

        _mediator = provider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task Send_ShouldReturnExpectedResponse()
    {
        var command = new PingCommand("ping");

        var response = await _mediator.Send(command);

        Assert.NotNull(response);

        Assert.Equal("ping => Pong!", response.Message);
    }

    [Fact]
    public async Task Publish_ShouldTriggerNotificationHandler()
    {
        var notification = new TestNotification("notify");

        await _mediator.Publish(notification);

        Assert.True(TestNotificationHandler.WasCalled); 
    }
}
