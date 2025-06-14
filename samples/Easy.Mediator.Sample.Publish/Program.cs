using Easy.Mediator;
using Easy.Mediator.Sample.Publish;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddEasyMediator(options =>
{
    options.AddAssembliesFrom("Easy.Mediator.Sample.Publish");
});

var provider = services.BuildServiceProvider();

var mediator = provider.GetRequiredService<IMediator>();

await mediator.Publish(new NewUserNotification("Bob", "Welcome to the system!"));
/*

[Email] To: Bob - Welcome to the system!
[Push] To: Bob - Welcome to the system!
[SMS] To: Bob - Welcome to the system!

*/
