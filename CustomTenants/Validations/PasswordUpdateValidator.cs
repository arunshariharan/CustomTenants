using CustomTenants.Models;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomTenants.Validations
{
    public class PasswordUpdateValidator : AbstractValidator<PasswordUpdate>
    {
        public PasswordUpdateValidator()
        {
            RuleFor(pass => pass.NewPassword)
                .NotEmpty().WithMessage("Password field must not be empty")
                .MinimumLength(8).WithMessage("Password must be minimum 8 characters long")
                .MaximumLength(45).WithMessage("Password length cannot exceed 45 characters");

        }
    }
}
