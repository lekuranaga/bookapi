using System.Text.RegularExpressions;
using BookTracker.Domain.Common;

namespace BookTracker.Domain.Users;

public sealed partial class Email : ValueObject
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) throw new DomainException("Invalid email.");
        var normalized = raw.Trim().ToLowerInvariant();
        if (!EmailRegex().IsMatch(normalized)) throw new DomainException("Invalid email.");
        return new Email(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }

    public override string ToString() => Value;

    // Pragmatic check, not RFC 5322.
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();
}
