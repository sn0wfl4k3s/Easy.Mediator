using System.Threading;
using System.Threading.Tasks;

namespace Easy.Mediator.UnitTests.Utils.Notifications;

public class TestNotificationHandler : INotificationHandler<TestNotification>
{
    public static bool WasCalled { get; set; } = false;
    public static string? ReceivedContent { get; set; }
    public Task Handle(TestNotification notification, CancellationToken cancellationToken = default)
    {
        WasCalled = true;
        ReceivedContent = notification.Content;
        return Task.CompletedTask;
    }
}