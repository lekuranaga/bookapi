using BookTracker.Application.Auth;
using BookTracker.Application.Books;
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
