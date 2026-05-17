namespace BookTracker.Application.Common.Exceptions;

public sealed class NotFoundException(string resource, Guid id)
    : AppException($"{resource} {id} not found.");
