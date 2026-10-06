using AstroBackend.Application.DTOs;
using FluentValidation;

namespace AstroBackend.Application.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Düzgün e-poçt ünvanı daxil edin.");
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("Şifrə ən azı 6 simvol olmalıdır.");
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(100).WithMessage("Ad-soyad mütləq daxil edilməlidir.");
        }
    }

    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }

    public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
    {
        public RefreshTokenRequestValidator()
        {
            RuleFor(x => x.RefreshToken).NotEmpty();
        }
    }

    public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordRequestValidator()
        {
            RuleFor(x => x.OldPassword).NotEmpty();
            RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6).WithMessage("Yeni şifrə ən azı 6 simvol olmalıdır.");
        }
    }

    public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
    {
        public ForgotPasswordRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }

    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Token).NotEmpty().WithMessage("Sıfırlama tokeni mütləq daxil edilməlidir.");
            RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6).WithMessage("Yeni şifrə ən azı 6 simvol olmalıdır.");
        }
    }
}
