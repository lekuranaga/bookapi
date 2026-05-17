namespace BookTracker.Application.Abstractions;

public interface ICurrentUser
{
    Guid Id { get; }
}
