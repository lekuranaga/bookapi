using BookTracker.Domain.Common;

namespace BookTracker.Domain.Books;

public sealed class Rating : ValueObject
{
    public const int Min = 1;
    public const int Max = 5;

    public int Value { get; }

    private Rating(int value) => Value = value;

    public static Rating Of(int value)
    {
        if (value < Min || value > Max)
            throw new DomainException($"Rating must be between {Min} and {Max}.");
        return new Rating(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }

    public override string ToString() => Value.ToString(System.Globalization.CultureInfo.InvariantCulture);

    public static implicit operator int(Rating r) => r.Value;
}
