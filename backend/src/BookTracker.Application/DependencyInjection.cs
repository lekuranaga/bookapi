using BookTracker.Application.Auth.Login;
using BookTracker.Application.Auth.Register;
using BookTracker.Application.Books.Create;
using BookTracker.Application.Books.Delete;
using BookTracker.Application.Books.Get;
using BookTracker.Application.Books.List;
using BookTracker.Application.Books.Update;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BookTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterUseCase>();
        services.AddScoped<LoginUseCase>();
        services.AddScoped<CreateBookUseCase>();
        services.AddScoped<UpdateBookUseCase>();
        services.AddScoped<DeleteBookUseCase>();
        services.AddScoped<GetBookUseCase>();
        services.AddScoped<ListBooksUseCase>();

        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
        return services;
    }
}
