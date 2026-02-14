# 🚀 Easy.Mediator

**Easy.Mediator** is a lightweight, extensible, and intuitive library for implementing the **Mediator pattern** in .NET applications. It helps you separate concerns, reduce coupling, and keep your code clean and testable.

---

## ✨ Features

- ✅ Supports **Requests/Responses** and **Notifications**
- ⚙️ Automatic handler registration via **Dependency Injection**
- 🔄 Compatible with **.NET Standard 2.1**, **.NET Core 2.1+** and **.NET 5+** or higher
- 🔌 Seamless integration with `Microsoft.Extensions.DependencyInjection`
- 📦 Easy to use  
- 🧩 Support for **Pipeline Behaviors** (interceptors like logging, validation, etc.)
- 💎 **100% Compatible with C# Records** (C# 9+) even though library is C# 7.3
- 📊 NEW: **Bounded Channel Capacity** for audit trail — control memory usage with optional channel size limits

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
public class NewUserNotification : INotification
{
    public string UserName { get; }
    public string Message { get; }

    public NewUserNotification(string userName, string message)
    {
        UserName = userName;
        Message = message;
    }
}
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
public class PingCommand : IRequest<PongResponse>
{
    public string Message { get; }
    public PingCommand(string message) => Message = message;
}

public class PongResponse
{
    public string Message { get; }
    public PongResponse(string message) => Message = message;
}
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

## 📊 Bounded Channel Capacity (Optional)

By default, Easy.Mediator uses **unbounded channels** for its audit trail — meaning audit data grows without limit as long as the application runs. Starting with **v2.2.0**, you can optionally set a **bounded capacity** to control how many audit items are kept in memory.

### Why use bounded channels?

| Benefit | Description |
|---|---|
| **Memory control** | Prevents the audit channel from growing indefinitely in long-running applications |
| **Predictable resource usage** | You define the exact maximum number of items held in memory |
| **Safe by default** | When the channel is full, the oldest items are automatically dropped (no blocking, no exceptions) |
| **Zero impact if unused** | If you don't call `SetChannelCapacity`, everything works exactly as before (unbounded) |

### Usage

Just chain `.SetChannelCapacity(n)` in your configuration:

```csharp
services.AddEasyMediator(options =>
{
    options.SetChannelCapacity(100); // keeps the last 100 audit items
});
```

Without it, the default behavior is unchanged:

```csharp
// Unbounded (default) — same as previous versions
services.AddEasyMediator();
```

### How it works

- When a capacity is set, the audit channels are created with `Channel.CreateBounded<T>` using `BoundedChannelFullMode.DropOldest`.
- This means `Send` and `Publish` **never block** — if the channel is full, the oldest audit entry is silently discarded to make room for the new one.
- When no capacity is set, channels remain `Channel.CreateUnbounded<T>` — identical to previous versions.

> ⚠️ Capacity must be greater than zero. Passing `0` or a negative value throws an `ArgumentOutOfRangeException`.

---

## 📃 License
This project is licensed under the MIT License.

You are free to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the software, provided that you include the original copyright
and license notice.

See the [License.txt](https://github.com/sn0wfl4k3s/Easy.Mediator/blob/master/LICENSE.txt) file for full details.

---

## 💎 C# Records Support

Easy.Mediator is **100% compatible with C# Records**! Even though the library is compiled with C# 7.3 for maximum compatibility, your projects can freely use C# 9+ Records.

### Using Records with Easy.Mediator

```csharp
// ✅ Your project can use C# 9+ Records
public record CreateOrderCommand(string OrderId, string ProductName) : IRequest<OrderResponse>;

public record OrderResponse(string OrderId, string ProductName, string ConfirmationId);

public record OrderCreatedNotification(string OrderId, string ProductName) : INotification;

// Handlers work seamlessly with records
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    public Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var response = new OrderResponse(
            request.OrderId,
            request.ProductName,
            Guid.NewGuid().ToString()
        );
        return Task.FromResult(response);
    }
}
```

### Compatibility Matrix

| Your Project | C# Version | .NET Target | Easy.Mediator | Status |
|--------------|-----------|------------|---------------|--------|
| Legacy | C# 7.3 | .NET Framework 4.7.2+ | ✅ Works |
| Modern | C# 8.0 | .NET Core 3.0+ | ✅ Works |
| Latest | C# 9.0+ | .NET 5.0+ | ✅ Works + Records |

Easy.Mediator is compiled for C# 7.3 and .NET Standard 2.1, ensuring maximum compatibility across all .NET versions and C# versions. Your code is free to use any newer features! 🎉

