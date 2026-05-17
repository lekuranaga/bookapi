using BookTracker.Application.Abstractions.Security;
using BookTracker.Application.Auth.Register;
using BookTracker.Application.Common.Exceptions;
using BookTracker.Domain.Users;
using FluentAssertions;
using FluentValidation;
using ValidationException = BookTracker.Application.Common.Exceptions.ValidationException;
using NSubstitute;

namespace BookTracker.Application.Tests.Auth;

public class RegisterUseCaseTests
{
    private readonly IValidator<RegisterRequest> _validator = new RegisterRequestValidator();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _tokens = Substitute.For<IJwtTokenGenerator>();

    private RegisterUseCase NewSut() => new(_validator, _users, _hasher, _tokens);

    [Fact]
    public async Task Execute_NewEmail_CreatesUserAndReturnsToken()
    {
        _users.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns((User?)null);
        _hasher.Hash("Passw0rd").Returns("hash");
        _tokens.Generate(Arg.Any<User>()).Returns(new AccessToken("jwt", DateTime.UtcNow.AddHours(1)));

        var result = await NewSut().ExecuteAsync(new RegisterRequest("a@b.com", "Passw0rd"), CancellationToken.None);

        result.AccessToken.Should().Be("jwt");
        result.Email.Should().Be("a@b.com");
        await _users.Received(1).AddAsync(Arg.Is<User>(u => u.Email.Value == "a@b.com" && u.PasswordHash == "hash"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_NormalizesEmailLowercase()
    {
        _users.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns((User?)null);
        _hasher.Hash(Arg.Any<string>()).Returns("h");
        _tokens.Generate(Arg.Any<User>()).Returns(new AccessToken("jwt", DateTime.UtcNow));

        await NewSut().ExecuteAsync(new RegisterRequest("A@B.COM", "Passw0rd"), CancellationToken.None);

        await _users.Received().FindByEmailAsync(Arg.Is<Email>(e => e.Value == "a@b.com"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_ExistingEmail_ThrowsConflict()
    {
        var existing = User.Register(Email.Create("a@b.com"), "h");
        _users.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns(existing);

        var act = () => NewSut().ExecuteAsync(new RegisterRequest("a@b.com", "Passw0rd"), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        await _users.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_WeakPassword_ThrowsValidation()
    {
        var act = () => NewSut().ExecuteAsync(new RegisterRequest("a@b.com", "weak"), CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }
}
