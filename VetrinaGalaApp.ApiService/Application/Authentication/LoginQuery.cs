﻿using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using VetrinaGalaApp.ApiService.Application.Common.Security;
using VetrinaGalaApp.ApiService.Application.Validators;
using VetrinaGalaApp.ApiService.EndPoints;
using VetrinaGalaApp.ApiService.Infrastructure.Models;

namespace VetrinaGalaApp.ApiService.Application.Authentication;

public record LoginQuery(string Email, string Password) : IRequest<ErrorOr<AuthenticationResult>>;
public class LoginQueryValidator : AbstractValidator<LoginQuery>
{
    public LoginQueryValidator()
    {
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.Password).PasswordValidator();
    }
}

public class LoginQueryHandler(
    UserManager<User> userManager,
    IJwtTokenGenerator jwtTokenGenerator) :
    IRequestHandler<LoginQuery, ErrorOr<AuthenticationResult>>
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;
    public async Task<ErrorOr<AuthenticationResult>> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        // Attempt to login
        if (await _userManager.FindByEmailAsync(request.Email) is not User user)
            return Error.Unauthorized(description: "Invalid emial or password");

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid) return Error.Unauthorized(description: "Invalid emial or password");

        var roles = await _userManager.GetRolesAsync(user);

        var token = _jwtTokenGenerator.GenerateToken(user, [.. roles]);

        return new AuthenticationResult(user.Id, user.Email!, token);
    }
}