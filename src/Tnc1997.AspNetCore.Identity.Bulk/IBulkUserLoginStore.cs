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
    /// Adds the external <see cref="UserLoginInfo"/>s to the specified <paramref name="users"/>.
    /// </summary>
    /// <param name="users">The users to add the logins to.</param>
    /// <param name="logins">The external <see cref="UserLoginInfo"/>s to add to the specified users.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    Task AddLoginsAsync(IEnumerable<TUser> users, IEnumerable<UserLoginInfo> logins, CancellationToken cancellationToken);
    
    /// <summary>
    /// Attempts to remove the provided logins from the specified <paramref name="users"/>.
    /// </summary>
    /// <param name="users">The users to remove the logins from.</param>
    /// <param name="loginProviders">The login providers whose information should be removed.</param>
    /// <param name="providerKeys">The keys given by the external login providers for the specified users.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    Task RemoveLoginsAsync(IEnumerable<TUser> users, IEnumerable<string> loginProviders, IEnumerable<string> providerKeys, CancellationToken cancellationToken);
}
