# 🚀 Easy.Mediator

**Easy.Mediator** is a lightweight, extensible, and intuitive library for implementing the **Mediator pattern** in .NET applications. It helps you separate concerns, reduce coupling, and keep your code clean and testable.

---

## ✨ Features

- ✅ Supports **Requests/Responses** and **Notifications** (Publish/Subscribe)
- ⚙️ Automatic handler registration via **Dependency Injection**
- 🔄 Compatible with **.NET Standard 2.1** and **.NET 5+** or higher
- 🔌 Seamless integration with `Microsoft.Extensions.DependencyInjection`
- 📦 Production-ready and test-friendly

---

## 📦 Installation

Install via the .NET CLI:

```bash
dotnet add package Easy.Mediator
```

## 🚀 Basic Usage

### 🔔 Handling Notifications

#### 1. Define a notification

```csharp
public record NewUserNotification(string UserName, string Message) : INotification;

```
#### 2. Implement a notification handler
```csharp
public class EmailNotificationHandler : INotificationHandler<NewUserNotification>
{
    public Task Handle(NewUserNotification notification, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[Email] To: {notification.UserName} - {notification.Message}");

        return Task.CompletedTask;
    }
}
```
#### 3. Publish the notification
```csharp
var services = new ServiceCollection();

services.AddEasyMediator();

var provider = services.BuildServiceProvider();

var mediator = provider.GetRequiredService<IMediator>();

await mediator.Publish(new NewUserNotification("Bob", "Bem-vindo!"));
//[Email] To: Bob - Welcome to the system!
```
### 📬 Handling Requests (Request/Response)

#### 1. Define a request and its response
```csharp
public record PingCommand(string Message) : IRequest<PongResponse>;

public record PongResponse(string Message);
```
#### 2. Implement a request handler
```csharp
public class PingCommandHandler : IRequestHandler<PingCommand, PongResponse>
{
    public Task<PongResponse> Handle(PingCommand request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new PongResponse($"{request.Message} => Pong!"));
    }
}
```
#### 3. Send the request and receive the response
```csharp
var response = await mediator.Send(new PingCommand("Ping!"));

Console.WriteLine(response.Message); // Ping! => Pong!
```

## 📃 License
This project is licensed under the MIT License.

You are free to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the software, provided that you include the original copyright
and license notice.

See the [License.txt](LICENSE.txt) file for full details.