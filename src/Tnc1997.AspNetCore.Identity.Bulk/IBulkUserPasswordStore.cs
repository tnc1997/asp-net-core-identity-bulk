using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tnc1997.AspNetCore.Identity.Bulk
{
    public interface IBulkUserPasswordStore<TUser>
        : IBulkUserStore<TUser>
        where TUser : class
    {
        /// <summary>
        /// Gets the password hashes for the specified <paramref name="users"/>.
        /// </summary>
        /// <param name="users">The users whose password hashes should be retrieved.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, the password hashes for the specified <paramref name="users"/>.</returns>
        Task<IEnumerable<string?>> GetPasswordHashesAsync(IEnumerable<TUser> users, CancellationToken cancellationToken);

        /// <summary>
        /// Sets the password hashes for the specified users.
        /// </summary>
        /// <param name="tuples">The users whose password hashes should be set and the password hashes to set.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task SetPasswordHashesAsync(IEnumerable<(TUser, string?)> tuples, CancellationToken cancellationToken);
    }
}
