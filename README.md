# Easy.Mediator

**Easy.Mediator** � uma biblioteca leve e extens�vel para implementar o padr�o Mediator em aplica��es .NET, facilitando a separa��o de responsabilidades e promovendo um c�digo desacoplado.

## Recursos

- Suporte a comandos (Request/Response) e notifica��es (Publish/Subscribe)
- Registro autom�tico de handlers via Dependency Injection
- Compat�vel com .NET Standard 2.1 e .NET 9
- Integra��o simples com Microsoft.Extensions.DependencyInjection

## Instala��o

Adicione a refer�ncia ao pacote em seu projeto:
dotnet add package Easy.Mediator
## Uso B�sico

### Implementando Notifications (Publish/Subscribe)

#### 1. Defina uma Notificationpublic record NewUserNotification(string UserName, string Message) : INotification;
#### 2. Implemente um NotificationHandlerpublic class EmailNotificationHandler : INotificationHandler<NewUserNotification>
{
    public Task Handle(NewUserNotification notification, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[Email] To: {notification.UserName} - {notification.Message}");
        return Task.CompletedTask;
    }
}
#### 3. Publique a Notificationvar services = new ServiceCollection();
services.AddEasyMediator();
var provider = services.BuildServiceProvider();
var mediator = provider.GetRequiredService<IMediator>();

await mediator.Publish(new NewUserNotification("Bob", "Bem-vindo!"));
### Implementando Requests (Request/Response)

#### 1. Defina um Request e um Responsepublic record PingCommand(string Message) : IRequest<PongResponse>;
public record PongResponse(string Message);
#### 2. Implemente um RequestHandlerpublic class PingCommandHandler : IRequestHandler<PingCommand, PongResponse>
{
    public Task<PongResponse> Handle(PingCommand request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new PongResponse($"{request.Message} => Pong!"));
    }
}
#### 3. Envie o Request e obtenha o Responsevar response = await mediator.Send(new PingCommand("Ping!"));
Console.WriteLine(response.Message); // Ping! => Pong!
## Exemplo de Sa�da
[Email] To: Bob - Bem-vindo!
[Push] To: Bob - Bem-vindo!
[SMS] To: Bob - Bem-vindo!
Ping! => Pong!
## Licen�a

MIT License. Veja o arquivo LICENSE.txt para mais detalhes.