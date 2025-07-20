using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tnc1997.AspNetCore.Identity.Bulk;

public interface IBulkUserEmailStore<TUser>
    : IBulkUserStore<TUser>
    where TUser : class
{
    /// <summary>
    /// Gets the emails for the specified <paramref name="users"/>.
    /// </summary>
    /// <param name="users">The users whose emails should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The task object containing the results of the asynchronous operation, the emails for the specified <paramref name="users"/>.</returns>
    Task<IEnumerable<string?>> GetEmailsAsync(IEnumerable<TUser> users, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the normalized emails for the specified <paramref name="users"/>.
    /// </summary>
    /// <param name="users">The users whose emails should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The task object containing the results of the asynchronous lookup operation, the normalized emails for the specified <paramref name="users"/>.</returns>
    Task<IEnumerable<string?>> GetNormalizedEmailsAsync(IEnumerable<TUser> users, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the <paramref name="emails"/> for the specified <paramref name="users"/>.
    /// </summary>
    /// <param name="users">The users whose emails should be set.</param>
    /// <param name="emails">The emails to set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    Task SetEmailsAsync(IEnumerable<TUser> users, IEnumerable<string?> emails, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the <paramref name="normalizedEmails"/> for the specified <paramref name="users"/>.
    /// </summary>
    /// <param name="users">The users whose normalized emails should be set.</param>
    /// <param name="normalizedEmails">The normalized emails to set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    Task SetNormalizedEmailsAsync(IEnumerable<TUser> users, IEnumerable<string?> normalizedEmails, CancellationToken cancellationToken);
}
