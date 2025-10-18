using Auth.API.Extensions;
using Auth.API.Helpers;
using Auth.API.Messages;
using Auth.API.Models;
using Auth.API.Services;
using Core.Messaging.Abstractions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PhoneNumbers;

namespace Auth.API.Features;

public sealed class CreateUser
{
    public sealed class Response : Result<ResponseDto>
    {
        public static implicit operator Response(ResponseDto successResult) =>
            new() { Value = successResult };

        public static implicit operator Response(DomainError errorResult) =>
            new() { Error = errorResult };
    }

    public sealed record ResponseDto(int UserId, string AccessToken, string RefreshToke);

    public sealed record Request(
        string Username,
        string Email,
        string PhoneNumber,
        string Password,
        string DisplayName,
        string Role,
        string? AvatarPath,
        UserClaim[]? Claims
    ) : IRequest<Response>;

    public sealed class Validator : AbstractValidator<Request>
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public Validator(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;

            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("Username is required")
                .MinimumLength(3)
                .WithMessage("Username too short");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required")
                .MinimumLength(8)
                .WithMessage("Password length must be greater than 8");

            RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Phone number is required");

            RuleFor(r => r.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Invalid email")
                .MustAsync(
                    async (request, email, cancellationToken) =>
                    {
                        if (string.IsNullOrEmpty(email))
                            return true;

                        return await EmailAndRoleIsValidAsync(
                            email,
                            request.Role,
                            cancellationToken
                        );
                    }
                )
                .WithMessage("Email already exists");

            When(
                r => !string.IsNullOrEmpty(r.PhoneNumber),
                () =>
                {
                    RuleFor(r => r.PhoneNumber)
                        .Matches(@"^\+\d{1,3}\s\d+$")
                        .WithMessage("Invalid phone number format")
                        .Must(phoneNumber =>
                        {
                            PhoneNumberUtil phoneNumberUtil = PhoneNumberUtil.GetInstance();

                            try
                            {
                                PhoneNumber numberProto = phoneNumberUtil.Parse(phoneNumber, null);
                                return phoneNumberUtil.IsValidNumber(numberProto);
                            }
                            catch (NumberParseException)
                            {
                                return false;
                            }
                        })
                        .WithMessage("Phone Number not valid");
                }
            );

            RuleFor(r => r.DisplayName)
                .NotEmpty()
                .WithMessage("Display Name is required")
                .MaximumLength(100)
                .WithMessage("Display Name must be less that 100 characters");

            RuleFor(r => r.Role)
                .NotEmpty()
                .WithMessage("Role is required")
                .MustAsync(
                    async (role, cancellationToken) =>
                    {
                        return await _roleManager.RoleExistsAsync(role);
                    }
                )
                .WithMessage("Role does not exist");

            RuleFor(r => r.Username)
                .MustAsync(
                    async (request, username, cancellationToken) =>
                    {
                        return await UsernameAndRoleIsValidAsync(
                            username,
                            request.Role,
                            cancellationToken
                        );
                    }
                )
                .WithMessage("Username already exists");
        }

        private async Task<bool> EmailAndRoleIsValidAsync(
            string email,
            string role,
            CancellationToken cancellationToken
        )
        {
            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                var existingUserRoles = await _userManager.GetRolesAsync(existingUser);
                return !existingUserRoles.Contains(role);
            }

            return true;
        }

        private async Task<bool> UsernameAndRoleIsValidAsync(
            string username,
            string role,
            CancellationToken cancellationToken
        )
        {
            var existingUser = await _userManager.FindByNameAsync(username);

            if (existingUser != null)
            {
                var existingUserRoles = await _userManager.GetRolesAsync(existingUser);
                return !existingUserRoles.Contains(role);
            }

            return true;
        }
    }

    public sealed class Handler(
        IValidator<Request> validator,
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        ITokenService tokenService,
        IEventPublisher eventPublisher,
        IHttpContextAccessor httpContextAccessor
    ) : IRequestHandler<Request, Response>
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
                return new ValidationError(validationResult.Errors);

            var user = new User
            {
                UserName = request.Username,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                DisplayName = request.DisplayName,
                AvatarPath = request.AvatarPath ?? "/images/default-avatar.png",
                RegisteredAt = DateTime.UtcNow,
            };

            var identityResult = await userManager.CreateAsync(user, request.Password);

            if (!identityResult.Succeeded)
            {
                return new ValidationError(identityResult.Errors);
            }

            var roleExists = await roleManager.RoleExistsAsync(request.Role);
            if (!roleExists)
            {
                return new NotFound("Specified role does not exist");
            }

            await userManager.AddToRoleAsync(user, request.Role);

            if (request.Claims?.Any() == true)
            {
                await userManager.AddClaimsAsync(
                    user,
                    request.Claims.Select(c => new System.Security.Claims.Claim(c.Type, c.Value))
                );
            }

            var (access, refresh) = await tokenService.GenerateTokensAsync(
                user,
                httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString()
                    ?? "unknown"
            );

            await eventPublisher.PublishAsync(
                new UserCreatedEvent(
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.PhoneNumber,
                    user.DisplayName,
                    request.Role
                ),
                "Auth.UserCreatedEvent",
                cancellationToken
            );

            return new ResponseDto(user.Id, access, refresh);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "/api/users",
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
