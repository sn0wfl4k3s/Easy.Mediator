# 🚀 Easy.Mediator

**Easy.Mediator** is a lightweight, extensible, and intuitive library for implementing the **Mediator pattern** in .NET applications. It helps you separate concerns, reduce coupling, and keep your code clean and testable.

---

## ✨ Features

- ✅ Supports **Requests/Responses** and **Notifications**
- ⚙️ Automatic handler registration via **Dependency Injection**
- 🔄 Compatible with **.NET Standard 2.1**, **.NET CORE** and **.NET 5+** or higher
- 🔌 Seamless integration with `Microsoft.Extensions.DependencyInjection`
- 📦 use-friendly  
- 🧩 NEW: Support for **Pipeline Behaviors** (interceptors like logging, validation, etc.)

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

## 🎛️ Pipeline Behaviors (Interceptors)

Easy.Mediator supports **pipeline behaviors** that allow you to intercept request execution before and/or after the handler. Useful for:

- Logging
- Validation
- Caching
- Handling exceptions
- Auditing

### ✅ Implementing a Behavior

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken,
        Func<TRequest, CancellationToken, Task<TResponse>> next)
    {
        Console.WriteLine($">>> Handling {typeof(TRequest).Name}");
        var response = await next(request, cancellationToken);
        Console.WriteLine($"<<< Handled {typeof(TRequest).Name}");
        return response;
    }
}
```

### 🔧 Registering Behaviors

Behaviors must be registered in the desired order:

```csharp
services.AddEasyMediator(cfg =>
{
    cfg.AddRequestHandlersFromAssembly(typeof(SomeHandler).Assembly);

    cfg.AddPipelineBehavior(typeof(LoggingBehavior<,>));
    cfg.AddPipelineBehavior(typeof(ValidationBehavior<,>));
});
```

> ℹ️ The registration order defines the execution flow — behaviors registered first run before later ones and the final handler.

### 🧪 Example of a ValidationBehavior

```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) =>
        _validators = validators;

    public async Task<TResponse> HandleAsync(
        TRequest request,
        CancellationToken ct,
        Func<TRequest, CancellationToken, Task<TResponse>> next)
    {
        var failures = _validators
            .Select(v => v.Validate(request))
            .SelectMany(r => r.Errors)
            .Where(f => f != null);

        if (failures.Any())
            throw new ValidationException(failures);

        return await next(request, ct);
    }
}
```

## 📃 License
This project is licensed under the MIT License.

You are free to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the software, provided that you include the original copyright
and license notice.

See the [License.txt](https://github.com/sn0wfl4k3s/Easy.Mediator/blob/master/LICENSE.txt) file for full details.
