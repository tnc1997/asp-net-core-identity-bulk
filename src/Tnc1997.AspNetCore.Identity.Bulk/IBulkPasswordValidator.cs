using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Tnc1997.AspNetCore.Identity.Bulk
{
    /// <summary>Provides an abstraction for password validation.</summary>
    /// <typeparam name="TUser">The type encapsulating a user.</typeparam>
    public interface IBulkPasswordValidator<TUser>
        where TUser : class
    {
        /// <summary>
        /// Validates the specified passwords as an asynchronous operation.
        /// </summary>
        /// <param name="manager">The <see cref="BulkUserManager{TUser}"/> that can be used to retrieve user properties.</param>
        /// <param name="tuples">The users whose passwords should be validated and the passwords supplied for validation.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/>s of the validation operation.</returns>
        Task<IEnumerable<IdentityResult>> ValidateAsync(BulkUserManager<TUser> manager, IEnumerable<(TUser, string?)> tuples);
    }
}
