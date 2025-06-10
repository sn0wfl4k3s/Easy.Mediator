# Easy.Mediator

**Easy.Mediator** é uma biblioteca leve e extensível para implementar o padrão Mediator em aplicações .NET, facilitando a separação de responsabilidades e promovendo um código desacoplado.

## Recursos

- Suporte a comandos (Request/Response) e notificações (Publish/Subscribe)
- Registro automático de handlers via Dependency Injection
- Compatível com .NET Standard 2.1 e .NET 9
- Integração simples com Microsoft.Extensions.DependencyInjection

## Instalação

Adicione a referência ao pacote em seu projeto:
```
dotnet add package Easy.Mediator
```
## Uso Básico

### Implementando Notifications (Publish/Subscribe)

#### 1. Defina uma Notification
```
public record NewUserNotification(string UserName, string Message) : INotification;
```
#### 2. Implemente um NotificationHandler
```
public class EmailNotificationHandler : INotificationHandler<NewUserNotification>
{
    public Task Handle(NewUserNotification notification, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[Email] To: {notification.UserName} - {notification.Message}");
        return Task.CompletedTask;
    }
}
```
#### 3. Publique a Notification
```
var services = new ServiceCollection();
services.AddEasyMediator();
var provider = services.BuildServiceProvider();
var mediator = provider.GetRequiredService<IMediator>();

await mediator.Publish(new NewUserNotification("Bob", "Bem-vindo!"));
```
### Implementando Requests (Request/Response)

#### 1. Defina um Request e um Response
```
public record PingCommand(string Message) : IRequest<PongResponse>;
public record PongResponse(string Message);
```
#### 2. Implemente um RequestHandler
```
public class PingCommandHandler : IRequestHandler<PingCommand, PongResponse>
{
    public Task<PongResponse> Handle(PingCommand request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new PongResponse($"{request.Message} => Pong!"));
    }
}
```
#### 3. Envie o Request e obtenha o Response
```
var response = await mediator.Send(new PingCommand("Ping!"));
Console.WriteLine(response.Message); // Ping! => Pong!
```

## Licença

MIT License. Veja o arquivo LICENSE.txt para mais detalhes.
