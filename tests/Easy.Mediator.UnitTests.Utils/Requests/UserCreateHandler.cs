using System.Threading;
using System.Threading.Tasks;

namespace Easy.Mediator.UnitTests.Utils.Requests;

public class UserCreateHandler : IRequestHandler<UserCreateCommand, string>
{
    public static bool WasCalled = false;

    public Task<string> Handle(UserCreateCommand request, CancellationToken cancellationToken)
    {
        WasCalled = true;

        return Task.FromResult("Created");
    }
}
