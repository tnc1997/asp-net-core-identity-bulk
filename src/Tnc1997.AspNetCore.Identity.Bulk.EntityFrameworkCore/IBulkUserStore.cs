using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Tnc1997.AspNetCore.Identity.Bulk.EntityFrameworkCore;

public interface IBulkUserStore<TUser>
    : IDisposable
    where TUser : class
{
    /// <summary>
    /// Creates the specified <paramref name="users"/> in the user store.
    /// </summary>
    /// <param name="users">The users to create.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/>s of the creation operation.</returns>
    Task<IEnumerable<IdentityResult>> CreateAsync(IEnumerable<TUser> users, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the specified <paramref name="users"/> from the user store.
    /// </summary>
    /// <param name="users">The users to delete.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/>s of the delete operation.</returns>
    Task<IEnumerable<IdentityResult>> DeleteAsync(IEnumerable<TUser> users, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the specified <paramref name="users"/> in the user store.
    /// </summary>
    /// <param name="users">The users to update.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/>s of the update operation.</returns>
    Task<IEnumerable<IdentityResult>> UpdateAsync(IEnumerable<TUser> users, CancellationToken cancellationToken);
}
