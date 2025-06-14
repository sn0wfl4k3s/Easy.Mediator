using Easy.Mediator;
using Easy.Mediator.Sample.Send;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// or use `services.AddEasyMediator()` to register all handlers in the current assembly
services.AddEasyMediator(options =>
{
    options
        .AddAssembliesFrom("Easy.Mediator.Sample.Send")
        .UseScopedServiceLifetime();
});

var provider = services.BuildServiceProvider();

var mediator = provider.GetRequiredService<IMediator>();

var response = await mediator.Send(new PingCommand("Ping!"));

// Ping! => Pong!
Console.WriteLine(response.Message);