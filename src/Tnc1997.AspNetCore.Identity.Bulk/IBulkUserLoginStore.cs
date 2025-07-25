using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Tnc1997.AspNetCore.Identity.Bulk;

public interface IBulkUserLoginStore<TUser>
    : IBulkUserStore<TUser>
    where TUser : class
{
    /// <summary>
    /// Adds the external <see cref="UserLoginInfo"/>s to the specified users.
    /// </summary>
    /// <param name="tuples">The users to add the logins to and the external <see cref="UserLoginInfo"/>s to add to the specified users.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    Task AddLoginsAsync(IEnumerable<(TUser, UserLoginInfo)> tuples, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the users associated with the specified login providers and provider keys.
    /// </summary>
    /// <param name="tuples">The login providers who provided the provider keys and the keys provided by the login providers to identify the users.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> for the asynchronous operation, containing the users, if any that matched the specified login providers and keys.</returns>
    Task<IEnumerable<TUser?>> FindByLoginsAsync(IEnumerable<(string, string)> tuples, CancellationToken cancellationToken);

    /// <summary>
    /// Removes the provided logins from the specified users.
    /// </summary>
    /// <param name="tuples">The users to remove the logins from and the login providers whose information should be removed and the keys given by the external login providers for the specified users.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    Task RemoveLoginsAsync(IEnumerable<(TUser, string, string)> tuples, CancellationToken cancellationToken);
}
