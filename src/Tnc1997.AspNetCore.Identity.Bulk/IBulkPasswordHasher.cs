using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Tnc1997.AspNetCore.Identity.Bulk
{
    /// <summary>Provides an abstraction for hashing passwords.</summary>
    /// <typeparam name="TUser">The type used to represent a user.</typeparam>
    public interface IBulkPasswordHasher<TUser>
        where TUser : class
    {
        /// <summary>
        /// Returns the hashed representations of the supplied passwords for the specified users.
        /// </summary>
        /// <param name="tuples">The users whose passwords are to be hashed and the passwords to hash.</param>
        /// <returns>The hashed representations of the supplied passwords for the specified users.</returns>
        IEnumerable<string> HashPasswords(IEnumerable<(TUser, string)> tuples);

        /// <summary>
        /// Returns the <see cref="PasswordVerificationResult"/>s indicating the results of the password hash comparisons.
        /// </summary>
        /// <remarks>
        /// Implementations of this method should be time-consistent.
        /// </remarks>
        /// <param name="tuples">The users whose passwords should be verified and the hash values for the users' stored passwords and the passwords supplied for comparison.</param>
        /// <returns>The <see cref="PasswordVerificationResult"/>s indicating the results of the password hash comparisons.</returns>
        IEnumerable<PasswordVerificationResult> VerifyHashedPasswords(IEnumerable<(TUser, string, string)> tuples);
    }
}
