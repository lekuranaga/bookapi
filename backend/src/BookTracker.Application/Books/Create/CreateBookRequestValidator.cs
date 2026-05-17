using FluentValidation;

namespace BookTracker.Application.Books.Create;

public sealed class CreateBookRequestValidator : AbstractValidator<CreateBookRequest>
{
    public CreateBookRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Author).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Review).MaximumLength(4000).When(x => x.Review != null);
        RuleFor(x => x.ReadAt).LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow));
    }
}
