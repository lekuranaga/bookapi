namespace BookTracker.Application.Common;

public abstract class AppException(string message) : Exception(message);

public sealed class NotFoundException(string resource, Guid id)
    : AppException($"{resource} {id} not found.");

public sealed class ConflictException(string message) : AppException(message);

public sealed class UnauthorizedException(string message) : AppException(message);

public sealed class ValidationException(IDictionary<string, string[]> errors)
    : AppException("One or more validation errors occurred.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = new Dictionary<string, string[]>(errors);
}
