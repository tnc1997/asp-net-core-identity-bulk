using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Tnc1997.AspNetCore.Identity.Bulk;

public class BulkPasswordHasher<TUser>(
    IPasswordHasher<TUser> inner)
    : IBulkPasswordHasher<TUser>
    where TUser : class
{
    public IEnumerable<string> HashPasswords(
        IEnumerable<(TUser, string)> tuples)
    {
        var hashes = new List<string>();

        foreach (var (user, password) in tuples)
        {
            var hash = inner.HashPassword(user, password);

            hashes.Add(hash);
        }

        return hashes;
    }

    public IEnumerable<PasswordVerificationResult> VerifyHashedPasswords(
        IEnumerable<(TUser, string, string)> tuples)
    {
        var results = new List<PasswordVerificationResult>();

        foreach (var (user, hashedPassword, providedPassword) in tuples)
        {
            var result = inner.VerifyHashedPassword(user, hashedPassword, providedPassword);

            results.Add(result);
        }

        return results;
    }
}
