using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tnc1997.AspNetCore.Identity.Bulk;

public interface IBulkUserEmailStore<TUser>
    : IBulkUserStore<TUser>
    where TUser : class
{
    /// <summary>
    /// Finds and returns the users, if any, who have the specified <see cref="normalizedEmails"/>.
    /// </summary>
    /// <param name="normalizedEmails">The normalized emails to search for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The task object containing the results of the asynchronous operation, the users matching the specified <paramref name="normalizedEmails"/> if they exist.</returns>
    Task<IEnumerable<TUser?>> FindByEmailsAsync(IEnumerable<string> normalizedEmails, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the flags indicating whether the email addresses for the specified <paramref name="users"/> have been verified, true if the email address is verified otherwise false.
    /// </summary>
    /// <param name="users">The users whose email confirmation statuses should be returned.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The task object containing the results of the asynchronous operation, the flags indicating whether the email addresses for the specified <paramref name="users"/> have been confirmed or not.
    /// </returns>
    Task<IEnumerable<bool>> GetEmailConfirmedAsync(IEnumerable<TUser> users, CancellationToken cancellationToken);

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
    /// Sets the emails for the specified users.
    /// </summary>
    /// <param name="tuples">The users whose emails should be set and the emails to set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    Task SetEmailsAsync(IEnumerable<(TUser, string?)> tuples, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the normalized emails for the specified users.
    /// </summary>
    /// <param name="tuples">The users whose normalized emails should be set and the normalized emails to set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    Task SetNormalizedEmailsAsync(IEnumerable<(TUser, string?)> tuples, CancellationToken cancellationToken);
}
