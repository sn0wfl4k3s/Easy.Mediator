using Easy.Mediator;
using Easy.Mediator.Sample.Send;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddEasyMediator();

var provider = services.BuildServiceProvider();

var mediator = provider.GetRequiredService<IMediator>();

var response = await mediator.Send(new PingCommand("Ping!"));

// Ping! => Pong!
Console.WriteLine(response.Message);