using Microsoft.Extensions.DependencyInjection;

namespace Easy.Mediator.UnitTests;

public class MediatorWithRecordsTests
{
    private readonly IMediator _mediator;

    public MediatorWithRecordsTests()
    {
        var services = new ServiceCollection();

        services.AddEasyMediator();

        var provider = services.BuildServiceProvider();

        _mediator = provider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task Send_WithRecord_ShouldReturnExpectedResponse()
    {
        // Arrange
        var command = new CreateOrderRecord("ORDER-001", "Widget");

        // Act
        var response = await _mediator.Send(command);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("ORDER-001", response.OrderId);
        Assert.Equal("Widget", response.ProductName);
        Assert.NotEmpty(response.ConfirmationId);
    }

    [Fact]
    public async Task Send_WithMultipleRecords_ShouldReturnCorrectData()
    {
        // Arrange
        var command1 = new CreateOrderRecord("ORD-123", "Laptop");
        var command2 = new CreateOrderRecord("ORD-456", "Mouse");

        // Act
        var response1 = await _mediator.Send(command1);
        var response2 = await _mediator.Send(command2);

        // Assert
        Assert.Equal("ORD-123", response1.OrderId);
        Assert.Equal("ORD-456", response2.OrderId);
        Assert.NotEqual(response1.ConfirmationId, response2.ConfirmationId);
    }

    [Theory]
    [InlineData("ORD-A", "Product-A")]
    [InlineData("ORD-B", "Product-B")]
    [InlineData("ORD-C", "Product-C")]
    public async Task Send_WithRecordTheory_ShouldHandleMultipleCases(string orderId, string productName)
    {
        // Arrange
        var command = new CreateOrderRecord(orderId, productName);

        // Act
        var response = await _mediator.Send(command);

        // Assert
        Assert.Equal(orderId, response.OrderId);
        Assert.Equal(productName, response.ProductName);
    }

    [Fact]
    public async Task Publish_WithRecord_ShouldTriggerHandler()
    {
        // Arrange
        OrderCreatedNotificationHandler.WasNotified = false;
        OrderCreatedNotificationHandler.ReceivedOrderId = null;

        var notification = new OrderCreatedRecord("ORDER-NOTIFY", "Keyboard");

        // Act
        await _mediator.Publish(notification);

        // Assert
        Assert.True(OrderCreatedNotificationHandler.WasNotified);
        Assert.Equal("ORDER-NOTIFY", OrderCreatedNotificationHandler.ReceivedOrderId);
    }

    [Fact]
    public async Task Publish_WithRecord_ShouldPreserveData()
    {
        // Arrange
        OrderCreatedNotificationHandler.WasNotified = false;
        OrderCreatedNotificationHandler.ReceivedOrderId = null;
        OrderCreatedNotificationHandler.ReceivedProductName = null;

        var notification = new OrderCreatedRecord("ORD-XYZ", "Monitor");

        // Act
        await _mediator.Publish(notification);

        // Assert
        Assert.True(OrderCreatedNotificationHandler.WasNotified);
        Assert.Equal("ORD-XYZ", OrderCreatedNotificationHandler.ReceivedOrderId);
        Assert.Equal("Monitor", OrderCreatedNotificationHandler.ReceivedProductName);
    }
}

// Record Request/Response
public record CreateOrderRecord(string OrderId, string ProductName) : IRequest<OrderCreatedResponseRecord>;

public record OrderCreatedResponseRecord(string OrderId, string ProductName, string ConfirmationId);

// Record Notification
public record OrderCreatedRecord(string OrderId, string ProductName) : INotification;

// Handler for record request
public class CreateOrderRecordHandler : IRequestHandler<CreateOrderRecord, OrderCreatedResponseRecord>
{
    public Task<OrderCreatedResponseRecord> Handle(CreateOrderRecord request, CancellationToken cancellationToken)
    {
        var response = new OrderCreatedResponseRecord(
            request.OrderId,
            request.ProductName,
            Guid.NewGuid().ToString()
        );

        return Task.FromResult(response);
    }
}

// Handler for record notification
public class OrderCreatedNotificationHandler : INotificationHandler<OrderCreatedRecord>
{
    public static bool WasNotified { get; set; }
    public static string ReceivedOrderId { get; set; }
    public static string ReceivedProductName { get; set; }

    public Task Handle(OrderCreatedRecord notification, CancellationToken cancellationToken)
    {
        WasNotified = true;
        ReceivedOrderId = notification.OrderId;
        ReceivedProductName = notification.ProductName;

        return Task.CompletedTask;
    }
}
