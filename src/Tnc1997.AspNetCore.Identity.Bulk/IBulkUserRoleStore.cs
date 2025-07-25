using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tnc1997.AspNetCore.Identity.Bulk;

public interface IBulkUserRoleStore<TUser>
    : IBulkUserStore<TUser>
    where TUser : class
{
    /// <summary>
    /// Adds the specified users to the named roles.
    /// </summary>
    /// <param name="tuples">The users to add to the named roles and the names of the roles to add the users to.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    Task AddToRolesAsync(IEnumerable<(TUser, string)> tuples, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the flags indicating whether the specified users are members of the given named roles.
    /// </summary>
    /// <param name="tuples">The users whose role membership should be checked and the names of the role to be checked.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the flags indicating whether the specified users are members of the named roles.
    /// </returns>
    Task<IEnumerable<bool>> AreInRolesAsync(IEnumerable<(TUser, string)> tuples, CancellationToken cancellationToken);

    /// <summary>
    /// Removes the specified users from the named roles.
    /// </summary>
    /// <param name="tuples">The users to remove the named roles from and the names of the roles to remove.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    Task RemoveFromRolesAsync(IEnumerable<(TUser, string)> tuples, CancellationToken cancellationToken);
}
