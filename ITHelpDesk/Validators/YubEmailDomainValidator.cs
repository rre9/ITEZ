using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITHelpDesk.Models;
using Microsoft.AspNetCore.Identity;

namespace ITHelpDesk.Validators;

public class YubEmailDomainValidator : UserValidator<ApplicationUser>
{
    public const string AllowedDomain = "@yub.com.sa";

    public YubEmailDomainValidator(IdentityErrorDescriber errors) : base(errors)
    {
    }

    public override async Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
    {
        var baseResult = await base.ValidateAsync(manager, user);
        var errors = baseResult.Succeeded
            ? new List<IdentityError>()
            : baseResult.Errors.ToList();

        if (string.IsNullOrWhiteSpace(user.Email) ||
            !user.Email.Trim().EndsWith(AllowedDomain, StringComparison.OrdinalIgnoreCase))
        {
            errors.Add(new IdentityError
            {
                Code = "InvalidEmailDomain",
                Description = $"Registration is restricted to {AllowedDomain} email addresses."
            });
        }

        return errors.Count == 0
            ? IdentityResult.Success
            : IdentityResult.Failed(errors.ToArray());
    }
}

