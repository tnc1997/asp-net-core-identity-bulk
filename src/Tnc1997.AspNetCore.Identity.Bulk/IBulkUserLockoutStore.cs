using System;
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
    /// Gets the last <see cref="DateTimeOffset"/>s the users' last lockouts expired, if any. Any time in the past indicates a user is not locked out.
    /// </summary>
    /// <param name="users">The users whose lockout end dates should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, the <see cref="DateTimeOffset"/>s containing the last time the users' lockouts expired, if any.</returns>
    Task<IEnumerable<DateTimeOffset?>> GetLockoutEndDatesAsync(IEnumerable<TUser> users, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the flags indicating if the specified users can be locked out.
    /// </summary>
    /// <param name="tuples">The users whose ability to be locked out should be set and the flags indicating if lock out can be enabled for the specified users.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    Task SetLockoutEnabledAsync(IEnumerable<(TUser, bool)> tuples, CancellationToken cancellationToken);

    /// <summary>
    /// Locks out the users until the specified end dates have passed. Setting an end date in the past immediately unlocks a user.
    /// </summary>
    /// <param name="tuples">The users whose lockout end dates should be set and the <see cref="DateTimeOffset"/>s after which the users' lockouts should end.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    Task SetLockoutEndDatesAsync(IEnumerable<(TUser, DateTimeOffset?)> tuples, CancellationToken cancellationToken);
}
