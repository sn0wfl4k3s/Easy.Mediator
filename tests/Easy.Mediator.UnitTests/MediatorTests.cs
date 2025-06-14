using Easy.Mediator.UnitTests.Notifications;
using Easy.Mediator.UnitTests.Requests;
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
        Assert.Equal("notify", TestNotificationHandler.ReceivedContent);
    }

    [Theory]
    [InlineData("ping", "ping => Pong!")]
    [InlineData("hello", "hello => Pong!")]
    [InlineData("", " => Pong!")]
    public async Task Send_ShouldReturnExpectedResponse_Theory(string input, string expectedMessage)
    {
        var command = new PingCommand(input);

        var response = await _mediator.Send(command);

        Assert.NotNull(response);
        Assert.Equal(expectedMessage, response.Message);
    }

    [Theory]
    [InlineData("notify1")]
    [InlineData("")]
    [InlineData("another notification")]
    public async Task Publish_ShouldTriggerNotificationHandler_Theory(string content)
    {
        // Reset static state before test
        TestNotificationHandler.WasCalled = false;
        TestNotificationHandler.ReceivedContent = null;

        var notification = new TestNotification(content);
        await _mediator.Publish(notification);

        Assert.True(TestNotificationHandler.WasCalled);
        Assert.Equal(content, TestNotificationHandler.ReceivedContent);
    }
}
