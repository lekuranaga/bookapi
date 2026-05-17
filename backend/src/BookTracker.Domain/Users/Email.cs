using System.Text.RegularExpressions;
using BookTracker.Domain.Common;

namespace BookTracker.Domain.Users;

public sealed partial class Email : ValueObject
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string raw)
    {
        if (!TryCreate(raw, out var email))
            throw new DomainException("Invalid email.");
        return email;
    }

    public static bool TryCreate(string? raw, out Email email)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            email = null!;
            return false;
        }
        var normalized = raw.Trim().ToLowerInvariant();
        if (!EmailRegex().IsMatch(normalized))
        {
            email = null!;
            return false;
        }
        email = new Email(normalized);
        return true;
    }

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }

    public override string ToString() => Value;

    // Pragmatic check, not RFC 5322.
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();
}
