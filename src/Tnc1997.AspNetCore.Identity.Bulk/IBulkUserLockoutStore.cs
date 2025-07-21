using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tnc1997.AspNetCore.Identity.Bulk;

public interface IBulkUserLockoutStore<TUser>
    : IBulkUserStore<TUser>
    where TUser : class
{
    /// <summary>
    /// Gets the flags indicating whether user lockout can be enabled for the specified <see cref="users"/>.
    /// </summary>
    /// <param name="users">The users whose ability to be locked out should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, true if a user can be locked out, otherwise false.</returns>
    Task<IEnumerable<bool>> GetLockoutEnabledAsync(IEnumerable<TUser> users, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the flags indicating if the specified users can be locked out.
    /// </summary>
    /// <param name="tuples">The users whose ability to be locked out should be set and the flags indicating if lock out can be enabled for the specified users.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    Task SetLockoutEnabledAsync(IEnumerable<(TUser, bool)> tuples, CancellationToken cancellationToken);
}
