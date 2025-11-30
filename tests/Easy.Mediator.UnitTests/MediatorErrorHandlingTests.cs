using Microsoft.Extensions.DependencyInjection;

namespace Easy.Mediator.UnitTests;

public class MediatorErrorHandlingTests
{
    [Fact]
    public async Task Send_Should_Throw_When_Handler_Not_Registered()
    {
        // Arrange - Create a fresh mediator WITHOUT any handlers
        var services = new ServiceCollection();
        services.AddSingleton<IMediator>(provider => new Mediator(provider));
        // Note: NOT calling AddEasyMediator() to avoid auto-discovery
        
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        // Use a request type that is guaranteed to NOT have a handler
        var unregisteredCommand = new NotRegisteredRequestCommand("test");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => mediator.Send(unregisteredCommand)
        );

        Assert.Contains("No handler registered for request type", exception.Message);
        Assert.Contains("NotRegisteredRequestCommand", exception.Message);
    }

    [Fact]
    public async Task Send_Should_Throw_With_Correct_Error_Message()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IMediator>(provider => new Mediator(provider));
        
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var unregisteredCommand = new AnotherNotRegisteredCommand("data");

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => mediator.Send(unregisteredCommand)
        );

        // Assert
        Assert.Equal("No handler registered for request type AnotherNotRegisteredCommand", exception.Message);
    }

    [Fact]
    public async Task Send_Should_Throw_Before_Attempting_Handler_Execution()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IMediator>(provider => new Mediator(provider));
        
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var command = new NotRegisteredRequestCommand("should-fail-before-handler");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => mediator.Send(command)
        );

        // Assert - Error should mention missing handler registration
        Assert.Contains("No handler registered", exception.Message);
    }

    [Fact]
    public async Task Send_Should_Include_Request_Type_Name_In_Error_Message()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IMediator>(provider => new Mediator(provider));
        
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var command = new NotRegisteredRequestCommand("test");

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => mediator.Send(command)
        );

        // Assert - Error message should contain the type name for debugging
        Assert.Contains(typeof(NotRegisteredRequestCommand).Name, exception.Message);
    }
}

// NOTE: These request types are intentionally NOT registered with handlers
// They are only used to test error handling when handlers are missing

public class NotRegisteredRequestCommand : IRequest<string>
{
    public string Data { get; }

    public NotRegisteredRequestCommand(string data)
    {
        Data = data;
    }
}

public class AnotherNotRegisteredCommand : IRequest<int>
{
    public string Data { get; }

    public AnotherNotRegisteredCommand(string data)
    {
        Data = data;
    }
}

// Intentionally commented out - these handlers should NOT be registered
// so we can test the "handler not found" error path
//
// public class NotRegisteredRequestCommandHandler : IRequestHandler<NotRegisteredRequestCommand, string>
// {
//     public Task<string> Handle(NotRegisteredRequestCommand request, CancellationToken cancellationToken)
//     {
//         return Task.FromResult($"Handled: {request.Data}");
//     }
// }
//
// public class AnotherNotRegisteredCommandHandler : IRequestHandler<AnotherNotRegisteredCommand, int>
// {
//     public Task<int> Handle(AnotherNotRegisteredCommand request, CancellationToken cancellationToken)
//     {
//         return Task.FromResult(42);
//     }
// }

