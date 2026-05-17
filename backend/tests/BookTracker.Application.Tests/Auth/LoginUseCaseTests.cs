using BookTracker.Application.Abstractions.Security;
using BookTracker.Application.Auth.Login;
using BookTracker.Application.Common.Exceptions;
using BookTracker.Domain.Users;
using FluentAssertions;
using FluentValidation;
using ValidationException = BookTracker.Application.Common.Exceptions.ValidationException;
using NSubstitute;

namespace BookTracker.Application.Tests.Auth;

public class LoginUseCaseTests
{
    private readonly IValidator<LoginRequest> _validator = new LoginRequestValidator();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _tokens = Substitute.For<IJwtTokenGenerator>();

    private LoginUseCase NewSut() => new(_validator, _users, _hasher, _tokens);

    [Fact]
    public async Task Execute_ValidCredentials_ReturnsToken()
    {
        var user = User.Register(Email.Create("a@b.com"), "hash");
        _users.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns(user);
        _hasher.Verify("Passw0rd", "hash").Returns(true);
        _tokens.Generate(user).Returns(new AccessToken("jwt", DateTime.UtcNow.AddHours(1)));

        var result = await NewSut().ExecuteAsync(new LoginRequest("a@b.com", "Passw0rd"), CancellationToken.None);

        result.AccessToken.Should().Be("jwt");
        result.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Execute_UnknownEmail_ThrowsUnauthorized()
    {
        _users.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = () => NewSut().ExecuteAsync(new LoginRequest("nope@b.com", "x"), CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Execute_WrongPassword_ThrowsUnauthorized()
    {
        var user = User.Register(Email.Create("a@b.com"), "hash");
        _users.FindByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns(user);
        _hasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var act = () => NewSut().ExecuteAsync(new LoginRequest("a@b.com", "wrong"), CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Execute_MalformedEmail_ThrowsValidation()
    {
        var act = () => NewSut().ExecuteAsync(new LoginRequest("garbage", "x"), CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();
    }
}
