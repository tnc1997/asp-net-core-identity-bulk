using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Tnc1997.AspNetCore.Identity.Bulk;

public class BulkPasswordValidator<TUser>(
    IdentityErrorDescriber? errors = null)
    : IBulkPasswordValidator<TUser>
    where TUser : class
{
    public IdentityErrorDescriber Describer { get; } = errors ?? new IdentityErrorDescriber();

    public Task<IEnumerable<IdentityResult>> ValidateAsync(
        BulkUserManager<TUser> manager,
        IEnumerable<(TUser, string?)> tuples)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(tuples);

        var options = manager.Options.Password;

        var results = new List<IdentityResult>();
        
        foreach (var (_, password) in tuples)
        {
            var errors = new List<IdentityError>();

            if (string.IsNullOrWhiteSpace(password) || password.Length < options.RequiredLength)
            {
                errors.Add(Describer.PasswordTooShort(options.RequiredLength));
            }

            if (options.RequireNonAlphanumeric && password?.All(IsLetterOrDigit) is not false)
            {
                errors.Add(Describer.PasswordRequiresNonAlphanumeric());
            }

            if (options.RequireDigit && password?.Any(IsDigit) is not true)
            {
                errors.Add(Describer.PasswordRequiresDigit());
            }

            if (options.RequireLowercase && password?.Any(IsLower) is not true)
            {
                errors.Add(Describer.PasswordRequiresLower());
            }

            if (options.RequireUppercase && password?.Any(IsUpper) is not true)
            {
                errors.Add(Describer.PasswordRequiresUpper());
            }

            if (options.RequiredUniqueChars >= 1 && password?.Distinct().Count() < options.RequiredUniqueChars)
            {
                errors.Add(Describer.PasswordRequiresUniqueChars(options.RequiredUniqueChars));
            }

            results.Add(errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success);
        }

        return Task.FromResult<IEnumerable<IdentityResult>>(results);
    }

    /// <summary>
    /// Returns a flag indicating whether the supplied character is a digit.
    /// </summary>
    /// <param name="c">The character to check if it is a digit.</param>
    /// <returns>True if the character is a digit, otherwise false.</returns>
    public virtual bool IsDigit(char c)
    {
        return c is >= '0' and <= '9';
    }

    /// <summary>
    /// Returns a flag indicating whether the supplied character is an ASCII letter or digit.
    /// </summary>
    /// <param name="c">The character to check if it is an ASCII letter or digit.</param>
    /// <returns>True if the character is an ASCII letter or digit, otherwise false.</returns>
    public virtual bool IsLetterOrDigit(char c)
    {
        return IsUpper(c) || IsLower(c) || IsDigit(c);
    }

    /// <summary>
    /// Returns a flag indicating whether the supplied character is a lower case ASCII letter.
    /// </summary>
    /// <param name="c">The character to check if it is a lower case ASCII letter.</param>
    /// <returns>True if the character is a lower case ASCII letter, otherwise false.</returns>
    public virtual bool IsLower(char c)
    {
        return c is >= 'a' and <= 'z';
    }

    /// <summary>
    /// Returns a flag indicating whether the supplied character is an upper case ASCII letter.
    /// </summary>
    /// <param name="c">The character to check if it is an upper case ASCII letter.</param>
    /// <returns>True if the character is an upper case ASCII letter, otherwise false.</returns>
    public virtual bool IsUpper(char c)
    {
        return c is >= 'A' and <= 'Z';
    }
}
