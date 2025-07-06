namespace Easy.Mediator.UnitTests.Utils.Requests;

public record UserCreateCommand(string? Name) : IRequest<string>;
