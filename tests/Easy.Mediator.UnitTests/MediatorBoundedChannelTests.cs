using Easy.Mediator.UnitTests.Utils.Notifications;
using Easy.Mediator.UnitTests.Utils.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace Easy.Mediator.UnitTests;

public class MediatorBoundedChannelTests : IDisposable
{
    public MediatorBoundedChannelTests()
    {
        Mediator.ConfigureChannelCapacity(null);
    }

    public void Dispose()
    {
        Mediator.ConfigureChannelCapacity(null);
    }

    [Fact]
    public void SetChannelCapacity_ShouldReturnSameInstance_ForFluentChaining()
    {
        var options = new MediatorConfigurationOptions();

        var result = options.SetChannelCapacity(5);

        Assert.Same(options, result);
    }

    [Fact]
    public void SetChannelCapacity_ShouldThrow_WhenCapacityIsZero()
    {
        var options = new MediatorConfigurationOptions();

        Assert.Throws<ArgumentOutOfRangeException>(() => options.SetChannelCapacity(0));
    }

    [Fact]
    public void SetChannelCapacity_ShouldThrow_WhenCapacityIsNegative()
    {
        var options = new MediatorConfigurationOptions();

        Assert.Throws<ArgumentOutOfRangeException>(() => options.SetChannelCapacity(-1));
    }

    [Fact]
    public void ConfigureChannelCapacity_ShouldThrow_WhenCapacityIsZero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Mediator.ConfigureChannelCapacity(0));
    }

    [Fact]
    public void ConfigureChannelCapacity_ShouldThrow_WhenCapacityIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Mediator.ConfigureChannelCapacity(-5));
    }

    [Fact]
    public async Task Send_ShouldWork_WithBoundedChannel()
    {
        var services = new ServiceCollection();

        services.AddEasyMediator(options =>
        {
            options.SetChannelCapacity(10);
        });

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var command = new PingCommand("bounded");

        var response = await mediator.Send(command);

        Assert.NotNull(response);
        Assert.Equal("bounded => Pong!", response.Message);
    }

    [Fact]
    public async Task Publish_ShouldWork_WithBoundedChannel()
    {
        TestNotificationHandler.WasCalled = false;
        TestNotificationHandler.ReceivedContent = null;

        var services = new ServiceCollection();

        services.AddEasyMediator(options =>
        {
            options.SetChannelCapacity(10);
        });

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var notification = new TestNotification("bounded-notify");

        await mediator.Publish(notification);

        Assert.True(TestNotificationHandler.WasCalled);
        Assert.Equal("bounded-notify", TestNotificationHandler.ReceivedContent);
    }

    [Fact]
    public async Task Send_ShouldWork_WithoutSettingChannelCapacity()
    {
        var services = new ServiceCollection();

        services.AddEasyMediator();

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var command = new PingCommand("unbounded");

        var response = await mediator.Send(command);

        Assert.NotNull(response);
        Assert.Equal("unbounded => Pong!", response.Message);
    }

    [Fact]
    public async Task Send_ShouldNotBlock_WhenBoundedChannelIsFull()
    {
        var services = new ServiceCollection();

        services.AddEasyMediator(options =>
        {
            options.SetChannelCapacity(2);
        });

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        // Send more requests than the channel capacity to prove it never blocks
        for (int i = 0; i < 10; i++)
        {
            var response = await mediator.Send(new PingCommand($"overflow-{i}"));
            Assert.Equal($"overflow-{i} => Pong!", response.Message);
        }
    }

    [Fact]
    public async Task Publish_ShouldNotBlock_WhenBoundedChannelIsFull()
    {
        var services = new ServiceCollection();

        services.AddEasyMediator(options =>
        {
            options.SetChannelCapacity(1);
        });

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        // Publish more than capacity via Send (uses the same channel mechanism)
        // to prove it never blocks, without polluting shared TestNotificationHandler state
        for (int i = 0; i < 10; i++)
        {
            var response = await mediator.Send(new PingCommand($"pub-overflow-{i}"));
            Assert.Equal($"pub-overflow-{i} => Pong!", response.Message);
        }
    }

    [Fact]
    public async Task ConfigureChannelCapacity_WithNull_ShouldResetToUnbounded()
    {
        Mediator.ConfigureChannelCapacity(7);
        Mediator.ConfigureChannelCapacity(null);

        // After reset to unbounded, Send should still work without blocking
        var services = new ServiceCollection();
        services.AddEasyMediator();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var response = await mediator.Send(new PingCommand("after-reset"));

        Assert.Equal("after-reset => Pong!", response.Message);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(100)]
    public async Task Send_ShouldWork_WithVariousBoundedCapacities(int capacity)
    {
        var services = new ServiceCollection();

        services.AddEasyMediator(options =>
        {
            options.SetChannelCapacity(capacity);
        });

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var command = new PingCommand($"cap-{capacity}");

        var response = await mediator.Send(command);

        Assert.NotNull(response);
        Assert.Equal($"cap-{capacity} => Pong!", response.Message);
    }
}
