using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Easy.Mediator.UnitTests.Utils.Pipelines;

public sealed class FluentValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);

        var validationFailures = await Task.WhenAll(_validators.Select(validator => validator.ValidateAsync(context)));

        var errors = validationFailures
            .Where(x => !x.IsValid)
            .SelectMany(x => x.Errors)
            .ToArray();

        if (errors.Any())
        {
            throw new ValidationException("Error while validate", errors);
        }

        return await next(cancellationToken);
    }
}