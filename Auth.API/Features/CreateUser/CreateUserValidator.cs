using Auth.API.Features.CreateUser;
using Auth.API.Models;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using PhoneNumbers;

namespace Auth.API.Features.CreateUser;

public sealed class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;

    public CreateUserValidator(UserManager<User> userManager, RoleManager<Role> roleManager)
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

                    return await EmailAndRoleIsValidAsync(email, request.Role, cancellationToken);
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
