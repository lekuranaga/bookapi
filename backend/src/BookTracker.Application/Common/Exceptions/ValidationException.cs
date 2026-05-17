namespace BookTracker.Application.Common.Exceptions;

public sealed class ValidationException(IDictionary<string, string[]> errors)
    : AppException("One or more validation errors occurred.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = new Dictionary<string, string[]>(errors);
}
