using Auth.API.DbContexts;
using Auth.API.Extensions;
using Auth.API.Helpers;
using Auth.API.Models.Constants;
using Auth.API.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneNumbers;

namespace Auth.API.Features;

public sealed class Login
{
    public sealed record Response(
        string AccessToken,
        string RefreshToken,
        string Role,
        ProfileData Profile
    );

    public sealed record ProfileData(
        string UserName,
        string Email,
        string DisplayName,
        string AvatarPath
    );

    public sealed record Request(
        string Username,
        string Password,
        string LoginMethod = "username-password"
    ) : IRequest<Result<Response>>;

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            When(
                    req => req.LoginMethod == LoginMethods.Phone,
                    () =>
                    {
                        RuleFor(req => req.Username)
                            .NotEmpty()
                            .WithMessage("Phone is required")
                            .Matches(@"^\+\d{1,3}\s\d+$")
                            .WithMessage("Invalid phone number format")
                            .Must(phoneNumber =>
                            {
                                PhoneNumberUtil phoneNumberUtil = PhoneNumberUtil.GetInstance();
                                try
                                {
                                    PhoneNumber numberProto = phoneNumberUtil.Parse(
                                        phoneNumber,
                                        null
                                    );
                                    return phoneNumberUtil.IsValidNumber(numberProto);
                                }
                                catch (NumberParseException)
                                {
                                    return false;
                                }
                            })
                            .WithMessage("Phone Number not valid");
                    }
                )
                .Otherwise(() =>
                {
                    RuleFor(request => request.Username)
                        .NotEmpty()
                        .WithMessage("Username is required");
                });

            When(
                    req => req.LoginMethod == LoginMethods.Phone,
                    () =>
                    {
                        RuleFor(request => request.Password)
                            .NotEmpty()
                            .WithMessage("OTP is required");
                    }
                )
                .Otherwise(() =>
                {
                    RuleFor(request => request.Password)
                        .NotEmpty()
                        .WithMessage("Password is required");
                });
        }
    }

    public sealed class Handler(
        AuthDbContext dbContext,
        ITokenService tokenService,
        IHttpContextAccessor httpContextAccessor
    ) : IRequestHandler<Request, Result<Response>>
    {
        public async Task<Result<Response>> Handle(
            Request request,
            CancellationToken cancellationToken
        )
        {
            var user = request.LoginMethod switch
            {
                LoginMethods.Phone => await dbContext
                    .Users.Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.PhoneNumber == request.Username, cancellationToken),

                _ => await dbContext
                    .Users.Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.UserName == request.Username, cancellationToken),
            };

            if (user is null)
            {
                return new NotFound("Invalid username or password");
            }

            // TODO: Password / OTP check
            //var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            //if (!passwordValid)
            //    return new ValidationError("Invalid username or password");

            var (access, refresh) = await tokenService.GenerateTokensAsync(
                user,
                httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString()
                    ?? "unknown"
            );

            // Find role (assuming one role for simplicity)
            var role = user.UserRoles?.FirstOrDefault()?.Role?.Name;

            var profile = new ProfileData(
                user.UserName,
                user.Email,
                user.DisplayName,
                user.AvatarPath
            );

            return new Response(access, refresh, role, profile);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "/api/users/login",
                    async ([FromBody] Request request, [FromServices] IMediator mediator) =>
                    {
                        var response = await mediator.Send(request);
                        return response.ToHttpResult();
                    }
                )
                .WithTags("Users");
        }
    }
}
