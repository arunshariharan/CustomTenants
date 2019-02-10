using MultiTenant.Models;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiTenant.Validations
{
    public class UserCredentialsValidator : AbstractValidator<UserCredentials>
    {
        public UserCredentialsValidator()
        {
            RuleFor(user => user.EmailAddress)
                .NotEmpty().WithMessage("Email address cannot be empty")
                .EmailAddress().WithMessage("A Valid email address must be provided");

            RuleFor(user => user.Password)
                .NotEmpty().WithMessage("Password field must not be empty")
                .MinimumLength(8).WithMessage("Password must be minimum 8 characters long")
                .MaximumLength(45).WithMessage("Password length cannot exceed 45 characters");

        }
    }
}
