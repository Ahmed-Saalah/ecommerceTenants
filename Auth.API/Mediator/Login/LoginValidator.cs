using Auth.API.Models.Constants;
using FluentValidation;
using PhoneNumbers;

namespace Auth.API.Mediator.Login;

public sealed class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
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
            )
            .Otherwise(() =>
            {
                RuleFor(request => request.Username).NotEmpty().WithMessage("Username is required");
            });

        When(
                req => req.LoginMethod == LoginMethods.Phone,
                () =>
                {
                    RuleFor(request => request.Password).NotEmpty().WithMessage("OTP is required");
                }
            )
            .Otherwise(() =>
            {
                RuleFor(request => request.Password).NotEmpty().WithMessage("Password is required");
            });
    }
}
