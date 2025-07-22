using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tnc1997.AspNetCore.Identity.Bulk;

public interface IBulkUserSecurityStampStore<TUser>
    : IBulkUserStore<TUser>
    where TUser : class
{
    /// <summary>
    /// Gets the security stamps for the specified <paramref name="users"/>.
    /// </summary>
    /// <param name="users">The users whose security stamps should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the security stamps for the specified <paramref name="users"/>.</returns>
    Task<IEnumerable<string?>> GetSecurityStampsAsync(IEnumerable<TUser> users, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the provided security stamps for the specified users.
    /// </summary>
    /// <param name="tuples">The users whose security stamps should be set and the security stamps to set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    Task SetSecurityStampsAsync(IEnumerable<(TUser, string?)> tuples, CancellationToken cancellationToken);
}
