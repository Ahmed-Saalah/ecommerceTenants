using Auth.API.Helpers;
using Auth.API.Models;
using Auth.API.Models.Constants;
using Auth.API.Services;
using Core.Context;
using Core.DataAccess.Abstractions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using PhoneNumbers;

namespace Auth.API.Mediator;

public sealed class CreateUser
{
    public sealed record ResponseDto(
        int UserId,
        string AccessToken,
        string RefreshToken,
        DateTime ExpiresIn
    );

    public class Response : Result<ResponseDto>
    {
        public static implicit operator Response(ResponseDto successResult) =>
            new() { Value = successResult };

        public static implicit operator Response(DomainError errorResult) =>
            new() { Error = errorResult };
    }

    public sealed record Request(
        string Username,
        string? Email,
        string PhoneNumber,
        string Password,
        string DisplayName,
        string Role,
        string? AvatarPath,
        int[]? TenantIds,
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
                .MinimumLength(6)
                .WithMessage("Password too weak");

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
                        {
                            return true;
                        }
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
                        .NotEmpty()
                        .WithMessage("Phone Number is required")
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

            RuleFor(r => r.Password)
                .NotEmpty()
                .WithMessage("Password is required")
                .MinimumLength(8)
                .WithMessage("Password length must be greater than 8");

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
                        return await _roleManager.RoleExistsAsync(roleName: role);
                    }
                )
                .WithMessage("Role does not exist");

            RuleFor(r => r.Username)
                .NotEmpty()
                .WithMessage("Username is required")
                .MinimumLength(3)
                .WithMessage("Username length must be greater than 3")
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

            RuleFor(r => r.TenantIds)
                .Must(tenantIds => tenantIds == null || tenantIds.All(id => id > 0))
                .WithMessage("Tenant ID must greater than 0");
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
                var roleMatches = existingUserRoles.Contains(role);

                return !roleMatches;
            }

            return true;
        }

        private async Task<bool> UsernameAndRoleIsValidAsync(
            string username,
            string role,
            CancellationToken cancellationToken
        )
        {
            var existingUser = await _userManager.FindByEmailAsync(username);

            if (existingUser != null)
            {
                var existingUserRoles = await _userManager.GetRolesAsync(existingUser);
                var roleMatches = existingUserRoles.Contains(role);

                return !roleMatches;
            }

            return true;
        }
    }

    public sealed class Handler(
        IEntityWriter<UserTenant> userTenantWriter,
        IValidator<Request> validator,
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        ITokenGenerator tokenGenerator,
        IContext context
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

            var role = context.CurrentUser?.Role;
            var id = context.CurrentUser?.Id;

            var isGuestUpgrade =
                context.CurrentUser?.Role == RoleConstants.Guest
                && context.CurrentUser?.Id > 0
                && request.Role == RoleConstants.Customer;

            if (isGuestUpgrade)
            {
                var guest = await userManager.FindByIdAsync(context.CurrentUser.Id.ToString());
                if (guest == null)
                    return new NotFound("Guest user not found");

                guest.UserName = user.UserName;
                guest.Email = user.Email;
                guest.PhoneNumber = user.PhoneNumber;
                guest.DisplayName = user.DisplayName;
                guest.AvatarPath = user.AvatarPath;
                guest.RegisteredAt = user.RegisteredAt;

                await userManager.UpdateAsync(guest);

                var resetToken = await userManager.GeneratePasswordResetTokenAsync(guest);
                await userManager.ResetPasswordAsync(guest, resetToken, request.Password);

                await userManager.RemoveFromRoleAsync(guest, RoleConstants.Guest);
                await userManager.AddToRoleAsync(guest, RoleConstants.Customer);

                var (AccessToken, RefreshToken, Expires) = tokenGenerator.Generate(guest);
                return new ResponseDto(guest.Id, AccessToken, RefreshToken, Expires);
            }

            var identityResult = await userManager.CreateAsync(user, request.Password);

            if (!identityResult.Succeeded)
            {
                return new ValidationError(identityResult.Errors);
            }

            var roleExists = await roleManager.RoleExistsAsync(request.Role);

            if (!roleExists)
                return new NotFound("Specified role does not exist");

            await userManager.AddToRoleAsync(user, request.Role);

            if (request.TenantIds is not null)
            {
                foreach (var tenantId in request.TenantIds.Distinct())
                    await userTenantWriter.AddAsync(
                        new UserTenant { UserId = user.Id, TenantId = tenantId }
                    );
            }

            if (request.Claims?.Any() == true)
            {
                await userManager.AddClaimsAsync(
                    user,
                    request.Claims.Select(_ => new System.Security.Claims.Claim(_.Type, _.Value))
                );
            }

            var (accessToken, refreshToken, expiresAt) = tokenGenerator.Generate(user);
            return new ResponseDto(user.Id, accessToken, refreshToken, expiresAt);
        }
    }
}
