using FluentValidation;
using ValidationException = BookTracker.Application.Common.ValidationException;

namespace BookTracker.Api.Validation;

public static class RequestValidator
{
    public static async Task EnsureValidAsync<T>(this IValidator<T> validator, T request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (result.IsValid) return;

        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        throw new ValidationException(errors);
    }
}
