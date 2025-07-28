using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Tnc1997.AspNetCore.Identity.Bulk;

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
    /// Finds and returns the users, if any, who have the specified <see cref="userIds"/>.
    /// </summary>
    /// <param name="userIds">The user IDs to search for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the users matching the specified <paramref name="userIds"/> if they exist.</returns>
    Task<IEnumerable<TUser?>> FindByIdsAsync(IEnumerable<string> userIds, CancellationToken cancellationToken);

    /// <summary>
    /// Finds and returns the users, if any, who have the specified <see cref="normalizedUserNames"/>.
    /// </summary>
    /// <param name="normalizedUserNames">The normalized names to search for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the users matching the specified <see cref="normalizedUserNames"/> if they exist.</returns>
    Task<IEnumerable<TUser?>> FindByNamesAsync(IEnumerable<string> normalizedUserNames, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the normalized names for the specified <paramref name="users"/>.
    /// </summary>
    /// <param name="users">The users whose normalized names should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the normalized names for the specified <paramref name="users"/>.</returns>
    Task<IEnumerable<string?>> GetNormalizedUserNamesAsync(IEnumerable<TUser> users, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the names for the specified <paramref name="users"/>.
    /// </summary>
    /// <param name="users">The users whose names should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the names for the specified <paramref name="users"/>.</returns>
    Task<IEnumerable<string?>> GetUserNamesAsync(IEnumerable<TUser> users, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the given normalized names for the specified users.
    /// </summary>
    /// <param name="tuples">The users whose normalized names should be set and the normalized names to set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    Task SetNormalizedUserNamesAsync(IEnumerable<(TUser, string?)> tuples, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the given names for the specified users.
    /// </summary>
    /// <param name="tuples">The users whose names should be set and the names to set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    Task SetUserNamesAsync(IEnumerable<(TUser, string?)> tuples, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the specified <paramref name="users"/> in the user store.
    /// </summary>
    /// <param name="users">The users to update.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/>s of the update operation.</returns>
    Task<IEnumerable<IdentityResult>> UpdateAsync(IEnumerable<TUser> users, CancellationToken cancellationToken);
}
