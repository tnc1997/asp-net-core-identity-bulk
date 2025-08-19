using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Tnc1997.AspNetCore.Identity.Bulk
{
    public class BulkPasswordHasher<TUser>
        : IBulkPasswordHasher<TUser>
        where TUser : class
    {
        private readonly IPasswordHasher<TUser> _inner;

        public BulkPasswordHasher(
            IPasswordHasher<TUser> inner)
        {
            _inner = inner;
        }

        public virtual IEnumerable<string> HashPasswords(
            IEnumerable<(TUser, string)> tuples)
        {
            var hashes = new List<string>();

            foreach (var (user, password) in tuples)
            {
                var hash = _inner.HashPassword(user, password);

                hashes.Add(hash);
            }

            return hashes;
        }

        public virtual IEnumerable<PasswordVerificationResult> VerifyHashedPasswords(
            IEnumerable<(TUser, string, string)> tuples)
        {
            var results = new List<PasswordVerificationResult>();

            foreach (var (user, hashedPassword, providedPassword) in tuples)
            {
                var result = _inner.VerifyHashedPassword(user, hashedPassword, providedPassword);

                results.Add(result);
            }

            return results;
        }
    }
}
