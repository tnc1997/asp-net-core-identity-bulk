using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Tnc1997.AspNetCore.Identity.Bulk
{
    /// <summary>
    /// Provides the APIs for managing users in a persistence store.
    /// </summary>
    /// <typeparam name="TUser">The type encapsulating a user.</typeparam>
    public class BulkUserManager<TUser>
        : IDisposable
        where TUser : class
    {
        private bool _disposed;

        private readonly IServiceProvider? _services;

        /// <summary>
        /// Provides the APIs for managing users in a persistence store.
        /// </summary>
        /// <typeparam name="TUser">The type encapsulating a user.</typeparam>
        public BulkUserManager(
            IBulkUserStore<TUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IBulkPasswordHasher<TUser> passwordHasher,
            IEnumerable<IBulkUserValidator<TUser>>? userValidators = null,
            IEnumerable<IBulkPasswordValidator<TUser>>? passwordValidators = null,
            IBulkLookupNormalizer? keyNormalizer = null,
            IdentityErrorDescriber? errors = null,
            IServiceProvider? services = null,
            ILogger<BulkUserManager<TUser>>? logger = null)
        {
            _services = services;

            ErrorDescriber = errors ?? new IdentityErrorDescriber();
            KeyNormalizer = keyNormalizer;
            Logger = logger;
            Options = optionsAccessor.Value;
            PasswordHasher = passwordHasher;
            PasswordValidators = passwordValidators ?? new List<IBulkPasswordValidator<TUser>>();
            SupportsUserEmail = store is IBulkUserEmailStore<TUser>;
            SupportsUserLockout = store is IBulkUserLockoutStore<TUser>;
            SupportsUserSecurityStamp = store is IBulkUserSecurityStampStore<TUser>;
            UserValidators = userValidators ?? new List<IBulkUserValidator<TUser>>();
            Store = store;
        }

        /// <summary>
        /// The <see cref="IdentityErrorDescriber"/> used to generate error messages.
        /// </summary>
        public virtual IdentityErrorDescriber ErrorDescriber { get; }

        /// <summary>
        /// The <see cref="IBulkLookupNormalizer"/> used to normalize things like user and role names.
        /// </summary>
        public virtual IBulkLookupNormalizer? KeyNormalizer { get; }

        /// <summary>
        /// The <see cref="ILogger"/> used to log messages from the manager.
        /// </summary>
        public virtual ILogger? Logger { get; }

        /// <summary>
        /// The <see cref="IdentityOptions"/> used to configure Identity.
        /// </summary>
        public virtual IdentityOptions Options { get; }

        /// <summary>
        /// The <see cref="IBulkPasswordHasher{TUser}"/> used to hash passwords.
        /// </summary>
        public virtual IBulkPasswordHasher<TUser> PasswordHasher { get; }

        /// <summary>
        /// The <see cref="IBulkPasswordValidator{TUser}"/>s used to validate passwords.
        /// </summary>
        public virtual IEnumerable<IBulkPasswordValidator<TUser>> PasswordValidators { get; }
    
        /// <summary>
        /// Gets a flag indicating whether the backing user store supports user emails.
        /// </summary>
        public virtual bool SupportsUserEmail { get; }
    
        /// <summary>
        /// Gets a flag indicating whether the backing user store supports user lock-outs.
        /// </summary>
        public virtual bool SupportsUserLockout { get; }

        /// <summary>
        /// Gets a flag indicating whether the backing user store supports security stamps.
        /// </summary>
        public virtual bool SupportsUserSecurityStamp { get; }

        /// <summary>
        /// The <see cref="IBulkUserValidator{TUser}"/>s used to validate users.
        /// </summary>
        public virtual IEnumerable<IBulkUserValidator<TUser>> UserValidators { get; }

        /// <summary>
        /// The cancellation token used to cancel operations.
        /// </summary>
        protected CancellationToken CancellationToken { get; } = CancellationToken.None;

        /// <summary>
        /// The persistence store the manager operates over.
        /// </summary>
        protected IBulkUserStore<TUser> Store { get; }

        /// <summary>
        /// Adds the external <see cref="UserLoginInfo"/>s to the specified users.
        /// </summary>
        /// <param name="tuples">The users to add the logins to and the external <see cref="UserLoginInfo"/>s to add to the specified users.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/>s
        /// of the operation.
        /// </returns>
        public virtual async Task<IEnumerable<IdentityResult>> AddLoginsAsync(
            IEnumerable<(TUser, UserLoginInfo)> tuples)
        {
            ThrowIfDisposed();
            if (tuples is null) throw new ArgumentNullException(nameof(tuples));

            var inner1 = new List<(TUser, UserLoginInfo)>();
            var logins1 = new List<(string, string)>();
            var users1 = new List<TUser>();

            foreach (var (user, login) in tuples)
            {
                inner1.Add((user, login));
                logins1.Add((login.LoginProvider, login.ProviderKey));
                users1.Add(user);
            }

            var results = new List<IdentityResult>();

            foreach (var user in await FindByLoginsAsync(logins1))
            {
                if (user is not null)
                {
                    results.Add(IdentityResult.Failed(ErrorDescriber.LoginAlreadyAssociated()));
                }
                else
                {
                    results.Add(IdentityResult.Success);
                }
            }

            var indexes2 = new List<int>();
            var inner2 = new List<(TUser, UserLoginInfo)>();
            var users2 = new List<TUser>();

            for (var i = 0; i < results.Count; i++)
            {
                if (results[i] is not { Succeeded: true })
                {
                    continue;
                }

                indexes2.Add(i);
                inner2.Add(inner1[i]);
                users2.Add(users1[i]);
            }

            await GetLoginStore().AddLoginsAsync(inner2, CancellationToken);

            foreach (var (index, result) in indexes2.Zip(await UpdateAsync(users2)))
            {
                results[index] = result;
            }

            return results;
        }

        /// <summary>
        /// Adds the passwords to the specified users only if the user does not already have a password.
        /// </summary>
        /// <param name="tuples">The users whose passwords should be set and the passwords to set.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/>s of the operation.</returns>
        public virtual async Task<IEnumerable<IdentityResult>> AddPasswordsAsync(
            IEnumerable<(TUser, string)> tuples)
        {
            ThrowIfDisposed();
            if (tuples is null) throw new ArgumentNullException(nameof(tuples));

            var inner1 = new List<(TUser, string, bool)>();
            var users1 = new List<TUser>();

            foreach (var (user, password) in tuples)
            {
                inner1.Add((user, password, true));
                users1.Add(user);
            }

            var results = new List<IdentityResult>();

            foreach (var passwordHash in await GetPasswordStore().GetPasswordHashesAsync(users1, CancellationToken))
            {
                if (!string.IsNullOrWhiteSpace(passwordHash))
                {
                    results.Add(IdentityResult.Failed(ErrorDescriber.UserAlreadyHasPassword()));
                }
                else
                {
                    results.Add(IdentityResult.Success);
                }
            }

            var indexes2 = new List<int>();
            var inner2 = new List<(TUser, string, bool)>();

            for (var i = 0; i < results.Count; i++)
            {
                if (results[i] is not { Succeeded: true })
                {
                    continue;
                }

                indexes2.Add(i);
                inner2.Add(inner1[i]);
            }

            foreach (var (index, result) in indexes2.Zip(await UpdatePasswordHashes(inner2)))
            {
                results[index] = result;
            }

            var indexes3 = new List<int>();
            var users3 = new List<TUser>();

            for (var i = 0; i < results.Count; i++)
            {
                if (results[i] is not { Succeeded: true })
                {
                    continue;
                }

                indexes3.Add(i);
                users3.Add(users1[i]);
            }

            foreach (var (index, result) in indexes3.Zip(await UpdateAsync(users3)))
            {
                results[index] = result;
            }

            return results;
        }

        /// <summary>
        /// Adds the specified users to the named roles.
        /// </summary>
        /// <param name="tuples">The users to add to the named roles and the names of the roles to add the users to.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/>s of the operation.</returns>
        public virtual async Task<IEnumerable<IdentityResult>> AddToRolesAsync(
            IEnumerable<(TUser, string)> tuples)
        {
            ThrowIfDisposed();
            if (tuples is null) throw new ArgumentNullException(nameof(tuples));

            var names1 = new List<string>();
            var users1 = new List<TUser>();

            foreach (var (user, name) in tuples)
            {
                names1.Add(name);
                users1.Add(user);
            }

            var results = new List<IdentityResult>();

            var i = 0;

            foreach (var isInRole in await GetUserRoleStore().AreInRolesAsync(users1.Zip(NormalizeNames(names1).OfType<string>().ToList()), CancellationToken))
            {
                if (isInRole)
                {
                    results.Add(IdentityResult.Failed(ErrorDescriber.UserAlreadyInRole(names1[i])));
                }
                else
                {
                    results.Add(IdentityResult.Success);
                }

                i += 1;
            }

            var indexes2 = new List<int>();
            var names2 = new List<string>();
            var users2 = new List<TUser>();

            for (i = 0; i < results.Count; i++)
            {
                if (results[i] is not { Succeeded: true })
                {
                    continue;
                }

                indexes2.Add(i);
                names2.Add(names1[i]);
                users2.Add(users1[i]);
            }

            await GetUserRoleStore().AddToRolesAsync(users2.Zip(NormalizeNames(names2).OfType<string>().ToList()), CancellationToken);

            foreach (var (index, result) in indexes2.Zip(await UpdateAsync(users2)))
            {
                results[index] = result;
            }

            return results;
        }

        /// <summary>
        /// Gets the flags indicating whether the emails for the specified <paramref name="users"/> have been verified, true if the email is verified otherwise false.
        /// </summary>
        /// <param name="users">The users whose email confirmation statuses should be returned.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, the flags indicating whether the emails for the specified <paramref name="users"/> have been confirmed or not.</returns>
        public virtual async Task<IEnumerable<bool>> AreEmailsConfirmedAsync(
            IEnumerable<TUser> users)
        {
            ThrowIfDisposed();
            if (users is null) throw new ArgumentNullException(nameof(users));

            return await GetEmailStore().GetEmailConfirmedAsync(users, CancellationToken);
        }

        /// <summary>
        /// Creates the specified <paramref name="users"/> in the backing store with no password.
        /// </summary>
        /// <param name="users">The users to create.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/>s of the operation.</returns>
        public virtual async Task<IEnumerable<IdentityResult>> CreateAsync(
            IEnumerable<TUser> users)
        {
            ThrowIfDisposed();
            if (users is null) throw new ArgumentNullException(nameof(users));

            var inner1 = new List<TUser>();

            foreach (var user in users)
            {
                inner1.Add(user);
            }

            await UpdateSecurityStampsInternal(inner1);

            var results = new List<IdentityResult>();

            foreach (var result in await ValidateUsersAsync(inner1))
            {
                results.Add(result);
            }

            var indexes2 = new List<int>();
            var inner2 = new List<TUser>();
            var lockoutEnabled2 = new List<(TUser, bool)>();

            for (var i = 0; i < results.Count; i++)
            {
                if (results[i] is not { Succeeded: true })
                {
                    continue;
                }

                indexes2.Add(i);
                inner2.Add(inner1[i]);
                lockoutEnabled2.Add((inner1[i], true));
            }

            if (Options.Lockout.AllowedForNewUsers && SupportsUserLockout)
            {
                await GetUserLockoutStore().SetLockoutEnabledAsync(lockoutEnabled2, CancellationToken);
            }
        
            await UpdateNormalizedUserNamesAsync(inner2);
            await UpdateNormalizedEmailsAsync(inner2);

            foreach (var (index, result) in indexes2.Zip(await Store.CreateAsync(inner2, CancellationToken)))
            {
                results[index] = result;
            }

            return results;
        }

        /// <summary>
        /// Deletes the specified <paramref name="users"/> from the backing store.
        /// </summary>
        /// <param name="users">The users to delete.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/>s of the operation.</returns>
        public virtual async Task<IEnumerable<IdentityResult>> DeleteAsync(
            IEnumerable<TUser> users)
        {
            ThrowIfDisposed();
            if (users is null) throw new ArgumentNullException(nameof(users));

            return await Store.DeleteAsync(users, CancellationToken);
        }

        /// <summary>
        /// Finds and returns the users, if any, who have the specified <see cref="emails"/>.
        /// </summary>
        /// <remarks>
        /// It is recommended that <see cref="UserOptions.RequireUniqueEmail"/> be set to true when using this method, otherwise the store may throw if there are users with duplicate emails.
        /// </remarks>
        /// <param name="emails">The emails to search for.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the users matching the specified <paramref name="emails"/> if they exist.</returns>
        public virtual async Task<IEnumerable<TUser?>> FindByEmailsAsync(
            IEnumerable<string> emails)
        {
            ThrowIfDisposed();
            if (emails is null) throw new ArgumentNullException(nameof(emails));

            var users = new List<List<TUser?>>();

            var normalizedEmails = NormalizeEmails(emails).OfType<string>().ToList();

            var i = 0;

            foreach (var user in await GetEmailStore().FindByEmailsAsync(normalizedEmails, CancellationToken))
            {
                if (i < users.Count)
                {
                    users[i].Add(user);
                }
                else
                {
                    users.Add(new List<TUser?> { user });
                }

                i += 1;
            }

            if (Options.Stores.ProtectPersonalData)
            {
                var keyRing = _services!.GetRequiredService<ILookupProtectorKeyRing>();
                var protector = _services!.GetRequiredService<ILookupProtector>();

                foreach (var keyId in keyRing.GetAllKeyIds())
                {
                    var protectedEmails = normalizedEmails.Select(normalizedEmail => protector.Protect(keyId, normalizedEmail)).ToList();

                    i = 0;

                    foreach (var user in await GetEmailStore().FindByEmailsAsync(protectedEmails, CancellationToken))
                    {
                        if (i < users.Count)
                        {
                            users[i].Add(user);
                        }
                        else
                        {
                            users.Add(new List<TUser?> { user });
                        }

                        i += 1;
                    }
                }
            }

            return users.Select(users => users.FirstOrDefault(user => user is not null));
        }

        /// <summary>
        /// Finds and returns the users, if any, who have the specified <paramref name="userIds"/>.
        /// </summary>
        /// <param name="userIds">The user IDs to search for.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the users matching the specified <paramref name="userIds"/> if they exist.</returns>
        public virtual Task<IEnumerable<TUser?>> FindByIdsAsync(
            IEnumerable<string> userIds)
        {
            ThrowIfDisposed();
            if (userIds is null) throw new ArgumentNullException(nameof(userIds));

            return Store.FindByIdsAsync(userIds, CancellationToken);
        }

        /// <summary>
        /// Retrieves the users associated with the specified external login providers and provider keys.
        /// </summary>
        /// <param name="tuples">The login providers who provided the provider keys and the keys provided by the login providers to identify the users.</param>
        /// <returns>The <see cref="Task"/> for the asynchronous operation, containing the users, if any that matched the specified login providers and keys.</returns>
        public virtual async Task<IEnumerable<TUser?>> FindByLoginsAsync(
            IEnumerable<(string, string)> tuples)
        {
            ThrowIfDisposed();
            if (tuples is null) throw new ArgumentNullException(nameof(tuples));

            return await GetLoginStore().FindByLoginsAsync(tuples, CancellationToken);
        }

        /// <summary>
        /// Finds and returns the users, if any, who have the specified <see cref="userNames"/>.
        /// </summary>
        /// <param name="userNames">The names to search for.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the users matching the specified <paramref name="userNames"/> if they exist.</returns>
        public virtual async Task<IEnumerable<TUser?>> FindByNamesAsync(
            IEnumerable<string> userNames)
        {
            ThrowIfDisposed();
            if (userNames is null) throw new ArgumentNullException(nameof(userNames));

            var users = new List<List<TUser?>>();

            var normalizedNames = NormalizeNames(userNames).OfType<string>().ToList();

            var i = 0;

            foreach (var user in await Store.FindByNamesAsync(normalizedNames, CancellationToken))
            {
                if (i < users.Count)
                {
                    users[i].Add(user);
                }
                else
                {
                    users.Add(new List<TUser?> { user });
                }

                i += 1;
            }

            if (Options.Stores.ProtectPersonalData)
            {
                var keyRing = _services!.GetRequiredService<ILookupProtectorKeyRing>();
                var protector = _services!.GetRequiredService<ILookupProtector>();

                foreach (var keyId in keyRing.GetAllKeyIds())
                {
                    var protectedNames = normalizedNames.Select(normalizedName => protector.Protect(keyId, normalizedName)).ToList();

                    i = 0;

                    foreach (var user in await Store.FindByNamesAsync(protectedNames, CancellationToken))
                    {
                        if (i < users.Count)
                        {
                            users[i].Add(user);
                        }
                        else
                        {
                            users.Add(new List<TUser?> { user });
                        }

                        i += 1;
                    }
                }
            }

            return users.Select(users => users.FirstOrDefault(user => user is not null));
        }

        /// <summary>
        /// Gets the emails for the specified <paramref name="users"/>.
        /// </summary>
        /// <param name="users">The users whose emails should be retrieved.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, the emails for the specified <paramref name="users"/>.</returns>
        public virtual async Task<IEnumerable<string?>> GetEmailsAsync(
            IEnumerable<TUser> users)
        {
            ThrowIfDisposed();
            if (users is null) throw new ArgumentNullException(nameof(users));

            return await GetEmailStore().GetEmailsAsync(users, CancellationToken);
        }
    
        /// <summary>
        /// Gets the flags indicating whether user lockout can be enabled for the specified <see cref="users"/>.
        /// </summary>
        /// <param name="users">The users whose ability to be locked out should be returned.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, true if a user can be locked out, otherwise false.</returns>
        public virtual async Task<IEnumerable<bool>> GetLockoutEnabledAsync(
            IEnumerable<TUser> users)
        {
            ThrowIfDisposed();
            if (users is null) throw new ArgumentNullException(nameof(users));

            return await GetUserLockoutStore().GetLockoutEnabledAsync(users, CancellationToken);
        }
    
        /// <summary>
        /// Gets the last <see cref="DateTimeOffset"/>s the users' last lockouts expired, if any. Any time in the past indicates a user is not locked out.
        /// </summary>
        /// <param name="users">The users whose lockout end dates should be retrieved.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, the <see cref="DateTimeOffset"/>s containing the last time the users' lockouts expired, if any.</returns>
        public virtual async Task<IEnumerable<DateTimeOffset?>> GetLockoutEndDatesAsync(
            IEnumerable<TUser> users)
        {
            ThrowIfDisposed();
            if (users is null) throw new ArgumentNullException(nameof(users));

            return await GetUserLockoutStore().GetLockoutEndDatesAsync(users, CancellationToken);
        }
    
        /// <summary>
        /// Gets the logins for the specified <param ref="users"/>.
        /// </summary>
        /// <param name="users">The users whose logins should be retrieved.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="UserLoginInfo"/>s for the specified <paramref name="users"/>.</returns>
        public virtual async Task<IEnumerable<IEnumerable<UserLoginInfo>>> GetLoginsAsync(
            IEnumerable<TUser> users)
        {
            ThrowIfDisposed();
            if (users is null) throw new ArgumentNullException(nameof(users));

            return await GetLoginStore().GetLoginsAsync(users, CancellationToken);
        }

        /// <summary>
        /// Gets the role names the specified <paramref name="users"/> belong to.
        /// </summary>
        /// <param name="users">The users whose role names should be retrieved.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the role names the specified <paramref name="users"/> belong to.</returns>
        public virtual async Task<IEnumerable<IEnumerable<string>>> GetRolesAsync(
            IEnumerable<TUser> users)
        {
            ThrowIfDisposed();
            if (users is null) throw new ArgumentNullException(nameof(users));

            return await GetUserRoleStore().GetRolesAsync(users, CancellationToken);
        }

        /// <summary>
        /// Gets the IDs for the specified <paramref name="users"/>.
        /// </summary>
        /// <param name="users">The users whose IDs should be retrieved.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, the IDs for the specified <paramref name="users"/>.</returns>
        public virtual async Task<IEnumerable<string>> GetUserIdsAsync(
            IEnumerable<TUser> users)
        {
            ThrowIfDisposed();
            if (users is null) throw new ArgumentNullException(nameof(users));

            return await Store.GetUserIdsAsync(users, CancellationToken);
        }

        /// <summary>
        /// Gets the names for the specified <paramref name="users"/>.
        /// </summary>
        /// <param name="users">The users whose names should be retrieved.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, the names for the specified <paramref name="users"/>.</returns>
        public virtual async Task<IEnumerable<string?>> GetUserNamesAsync(
            IEnumerable<TUser> users)
        {
            ThrowIfDisposed();
            if (users is null) throw new ArgumentNullException(nameof(users));

            return await Store.GetUserNamesAsync(users, CancellationToken);
        }

        /// <summary>
        /// Gets the users who are members of the named roles.
        /// </summary>
        /// <param name="roleNames">The names of the roles whose memberships should be returned.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the users who are members of the named roles.</returns>
        public virtual Task<IEnumerable<IEnumerable<TUser>>> GetUsersInRolesAsync(
            IEnumerable<string> roleNames)
        {
            ThrowIfDisposed();
            if (roleNames is null) throw new ArgumentNullException(nameof(roleNames));

            return GetUserRoleStore().GetUsersInRolesAsync(NormalizeNames(roleNames).OfType<string>(), CancellationToken);
        }

        /// <summary>
        /// Normalizes emails for consistent comparisons.
        /// </summary>
        /// <param name="emails">The emails to normalize.</param>
        /// <returns>The normalized values representing the specified <paramref name="emails"/>.</returns>
        public virtual IEnumerable<string?> NormalizeEmails(
            IEnumerable<string?> emails)
        {
            ThrowIfDisposed();
            if (emails is null) throw new ArgumentNullException(nameof(emails));

            return KeyNormalizer?.NormalizeEmails(emails) ?? emails;
        }

        /// <summary>
        /// Normalizes user or role names for consistent comparisons.
        /// </summary>
        /// <param name="names">The names to normalize.</param>
        /// <returns>The normalized values representing the specified <paramref name="names"/>.</returns>
        public virtual IEnumerable<string?> NormalizeNames(
            IEnumerable<string?> names)
        {
            ThrowIfDisposed();
            if (names is null) throw new ArgumentNullException(nameof(names));

            return KeyNormalizer?.NormalizeNames(names) ?? names;
        }

        /// <summary>
        /// Removes the specified users from the named roles.
        /// </summary>
        /// <param name="tuples">The users to remove the named roles from and the names of the roles to remove.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/>s of the operation.</returns>
        public virtual async Task<IEnumerable<IdentityResult>> RemoveFromRoleAsync(
            IEnumerable<(TUser, string)> tuples)
        {
            ThrowIfDisposed();
            if (tuples is null) throw new ArgumentNullException(nameof(tuples));

            var names1 = new List<string>();
            var users1 = new List<TUser>();

            foreach (var (user, name) in tuples)
            {
                names1.Add(name);
                users1.Add(user);
            }

            var results = new List<IdentityResult>();

            var i = 0;

            foreach (var isInRole in await GetUserRoleStore().AreInRolesAsync(users1.Zip(NormalizeNames(names1).OfType<string>().ToList()), CancellationToken))
            {
                if (!isInRole)
                {
                    results.Add(IdentityResult.Failed(ErrorDescriber.UserNotInRole(names1[i])));
                }
                else
                {
                    results.Add(IdentityResult.Success);
                }

                i += 1;
            }

            var indexes2 = new List<int>();
            var names2 = new List<string>();
            var users2 = new List<TUser>();

            for (i = 0; i < results.Count; i++)
            {
                if (results[i] is not { Succeeded: true })
                {
                    continue;
                }

                indexes2.Add(i);
                names2.Add(names1[i]);
                users2.Add(users1[i]);
            }

            await GetUserRoleStore().RemoveFromRolesAsync(users2.Zip(NormalizeNames(names2).OfType<string>().ToList()), CancellationToken);

            foreach (var (index, result) in indexes2.Zip(await UpdateAsync(users2)))
            {
                results[index] = result;
            }

            return results;
        }

        /// <summary>
        /// Removes the provided logins from the specified users.
        /// </summary>
        /// <param name="tuples">The users to remove the logins from and the login providers whose information should be removed and the keys given by the external login providers for the specified users.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/>s of the operation.</returns>
        public virtual async Task<IEnumerable<IdentityResult>> RemoveLoginsAsync(
            IEnumerable<(TUser, string, string)> tuples)
        {
            ThrowIfDisposed();
            if (tuples is null) throw new ArgumentNullException(nameof(tuples));

            var inner1 = new List<(TUser, string, string)>();
            var users1 = new List<TUser>();

            foreach (var (user, loginProvider, providerKey) in tuples)
            {
                inner1.Add((user, loginProvider, providerKey));
                users1.Add(user);
            }

            await GetLoginStore().RemoveLoginsAsync(inner1, CancellationToken);

            await UpdateSecurityStampsInternal(users1);

            return await UpdateAsync(users1);
        }

        /// <summary>
        /// Sets the emails for the specified users.
        /// </summary>
        /// <param name="tuples">The users whose emails should be set and the emails to set.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/>s of the operation.</returns>
        public virtual async Task<IEnumerable<IdentityResult>> SetEmailsAsync(
            IEnumerable<(TUser, string?)> tuples)
        {
            ThrowIfDisposed();
            if (tuples is null) throw new ArgumentNullException(nameof(tuples));

            var emails = new List<(TUser, string?)>();
            var confirmed = new List<(TUser, bool)>();
            var users = new List<TUser>();

            foreach (var (user, email) in tuples)
            {
                emails.Add((user, email));
                confirmed.Add((user, false));
                users.Add(user);
            }

            await GetEmailStore().SetEmailsAsync(emails, CancellationToken);
            await GetEmailStore().SetEmailConfirmedAsync(confirmed, CancellationToken);

            await UpdateSecurityStampsInternal(users);

            return await UpdateAsync(users);
        }

        /// <summary>
        /// Sets the flags indicating if the specified users can be locked out.
        /// </summary>
        /// <param name="tuples">The users whose ability to be locked out should be set and the flags indicating if lock out can be enabled for the specified users.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, the <see cref="IdentityResult"/>s of the operation.</returns>
        public virtual async Task<IEnumerable<IdentityResult>> SetLockoutEnabledAsync(
            IEnumerable<(TUser, bool)> tuples)
        {
            ThrowIfDisposed();
            if (tuples is null) throw new ArgumentNullException(nameof(tuples));

            var inner1 = new List<(TUser, bool)>();
            var users1 = new List<TUser>();

            foreach (var (user, enabled) in tuples)
            {
                inner1.Add((user, enabled));
                users1.Add(user);
            }

            await GetUserLockoutStore().SetLockoutEnabledAsync(inner1, CancellationToken);

            return await UpdateAsync(users1);
        }

        /// <summary>
        /// Locks out the users until the specified end dates have passed. Setting an end date in the past immediately unlocks a user.
        /// </summary>
        /// <param name="tuples">The users whose lockout end dates should be set and the <see cref="DateTimeOffset"/>s after which the users' lockouts should end.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/>s of the operation.</returns>
        public virtual async Task<IEnumerable<IdentityResult>> SetLockoutEndDatesAsync(
            IEnumerable<(TUser, DateTimeOffset?)> tuples)
        {
            ThrowIfDisposed();
            if (tuples is null) throw new ArgumentNullException(nameof(tuples));

            var inner1 = new List<(TUser, DateTimeOffset?)>();
            var users1 = new List<TUser>();

            foreach (var (user, date) in tuples)
            {
                inner1.Add((user, date));
                users1.Add(user);
            }

            var results = new List<IdentityResult>();

            foreach (var isLockoutEnabled in await GetUserLockoutStore().GetLockoutEnabledAsync(users1, CancellationToken))
            {
                if (!isLockoutEnabled)
                {
                    results.Add(IdentityResult.Failed(ErrorDescriber.UserLockoutNotEnabled()));
                }
                else
                {
                    results.Add(IdentityResult.Success);
                }
            }

            var indexes2 = new List<int>();
            var inner2 = new List<(TUser, DateTimeOffset?)>();
            var users2 = new List<TUser>();

            for (var i = 0; i < results.Count; i++)
            {
                if (results[i] is not { Succeeded: true })
                {
                    continue;
                }

                indexes2.Add(i);
                inner2.Add(inner1[i]);
                users2.Add(users1[i]);
            }

            await GetUserLockoutStore().SetLockoutEndDatesAsync(inner2, CancellationToken);

            foreach (var (index, result) in indexes2.Zip(await UpdateAsync(users2)))
            {
                results[index] = result;
            }

            return results;
        }

        /// <summary>
        /// Sets the given names for the specified users.
        /// </summary>
        /// <param name="tuples">The users whose names should be set and the names to set.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/>s of the operation.</returns>
        public virtual async Task<IEnumerable<IdentityResult>> SetUserNamesAsync(
            IEnumerable<(TUser, string?)> tuples)
        {
            ThrowIfDisposed();
            if (tuples is null) throw new ArgumentNullException(nameof(tuples));

            var inner1 = new List<(TUser, string?)>();
            var users1 = new List<TUser>();

            foreach (var (user, userName) in tuples)
            {
                inner1.Add((user, userName));
                users1.Add(user);
            }

            await Store.SetUserNamesAsync(inner1, CancellationToken);

            await UpdateSecurityStampsInternal(users1);

            return await UpdateAsync(users1);
        }

        /// <summary>
        /// Updates the specified <paramref name="users"/> in the backing store.
        /// </summary>
        /// <param name="users">The users to update.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/>s of the operation.</returns>
        public virtual async Task<IEnumerable<IdentityResult>> UpdateAsync(
            IEnumerable<TUser> users)
        {
            ThrowIfDisposed();
            if (users is null) throw new ArgumentNullException(nameof(users));

            var inner1 = new List<TUser>();

            foreach (var user in users)
            {
                inner1.Add(user);
            }

            var results = new List<IdentityResult>();

            foreach (var result in await ValidateUsersAsync(inner1))
            {
                results.Add(result);
            }

            var indexes2 = new List<int>();
            var inner2 = new List<TUser>();

            for (var i = 0; i < results.Count; i++)
            {
                if (results[i] is not { Succeeded: true })
                {
                    continue;
                }

                indexes2.Add(i);
                inner2.Add(inner1[i]);
            }

            await UpdateNormalizedUserNamesAsync(inner2);
            await UpdateNormalizedEmailsAsync(inner2);

            foreach (var (index, result) in indexes2.Zip(await Store.UpdateAsync(inner2, CancellationToken)))
            {
                results[index] = result;
            }

            return results;
        }

        /// <summary>
        /// Updates the normalized emails for the specified <paramref name="users"/>.
        /// </summary>
        /// <param name="users">The users whose email addresses should be normalized and updated.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public virtual async Task UpdateNormalizedEmailsAsync(
            IEnumerable<TUser> users)
        {
            ThrowIfDisposed();
            if (users is null) throw new ArgumentNullException(nameof(users));

            if (Store is IBulkUserEmailStore<TUser> store)
            {
                var normalizedEmails = await GetEmailsAsync(users);
                normalizedEmails = NormalizeEmails(normalizedEmails);
                normalizedEmails = ProtectPersonalData(normalizedEmails);

                await store.SetNormalizedEmailsAsync(users.Zip(normalizedEmails), CancellationToken);
            }
        }

        /// <summary>
        /// Updates the normalized names for the specified <paramref name="users"/>.
        /// </summary>
        /// <param name="users">The users whose names should be normalized and updated.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        public virtual async Task UpdateNormalizedUserNamesAsync(
            IEnumerable<TUser> users)
        {
            ThrowIfDisposed();
            if (users is null) throw new ArgumentNullException(nameof(users));

            var normalizedNames = await GetUserNamesAsync(users);
            normalizedNames = NormalizeNames(normalizedNames);
            normalizedNames = ProtectPersonalData(normalizedNames);

            await Store.SetNormalizedUserNamesAsync(users.Zip(normalizedNames), CancellationToken);
        }

        public void Dispose()
        {
            Store.Dispose();

            _disposed = true;

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Throws if this class has been disposed.
        /// </summary>
        protected void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        /// <summary>
        /// Updates the passwords for the specified users.
        /// </summary>
        /// <param name="tuples">The users whose passwords should be set and the passwords to set and whether to validate the passwords.</param>
        /// <returns>The <see cref="IdentityResult"/>s representing whether the updates were successful.</returns>
        protected virtual async Task<IEnumerable<IdentityResult>> UpdatePasswordHashes(
            IEnumerable<(TUser, string?, bool)> tuples)
        {
            var results = new List<IdentityResult>();

            var inner1 = new List<(TUser, string?)>();

            var indexes2 = new List<int>();
            var passwords2 = new List<(TUser, string?)>();

            var i = 0;

            foreach (var (user, password, validatePassword) in tuples)
            {
                results.Add(IdentityResult.Success);

                inner1.Add((user, password));

                if (validatePassword)
                {
                    indexes2.Add(i);
                    passwords2.Add((user, password));
                }

                i += 1;
            }

            if (passwords2.Count > 0)
            {
                foreach (var (index, result) in indexes2.Zip(await ValidatePasswordsAsync(passwords2)))
                {
                    results[index] = result;
                }
            }

            var inner3 = new List<(TUser, string?)>();
            var users3 = new List<TUser>();

            var indexes4 = new List<int>();
            var passwords4 = new List<(TUser, string)>();

            for (i = 0; i < results.Count; i++)
            {
                if (results[i] is not { Succeeded: true })
                {
                    continue;
                }

                inner3.Add(inner1[i]);
                users3.Add(inner1[i].Item1);

                if (!string.IsNullOrWhiteSpace(inner1[i].Item2))
                {
                    indexes4.Add(i);
                    passwords4.Add((inner1[i].Item1, inner1[i].Item2!));
                }
            }

            foreach (var (index, hash) in indexes4.Zip(PasswordHasher.HashPasswords(passwords4)))
            {
                inner3[index] = (inner3[index].Item1, hash);
            }

            await GetPasswordStore().SetPasswordHashesAsync(inner3, CancellationToken);
        
            await UpdateSecurityStampsInternal(users3);

            return results;
        }

        /// <summary>
        /// Validates the passwords for the specified users.
        /// </summary>
        /// <param name="tuples">The users whose passwords should be validated and the passwords to validate</param>
        /// <returns>The <see cref="IdentityResult"/>s representing whether the updates were successful.</returns>
        protected async Task<IEnumerable<IdentityResult>> ValidatePasswordsAsync(
            IEnumerable<(TUser, string?)> tuples)
        {
            if (PasswordValidators.Any())
            {
                var errors = new List<List<IdentityError>>();

                foreach (var validator in PasswordValidators)
                {
                    var i = 0;

                    foreach (var result in await validator.ValidateAsync(this, tuples))
                    {
                        if (i < errors.Count)
                        {
                            errors[i].AddRange(result.Errors);
                        }
                        else
                        {
                            errors.Add(result.Errors.ToList());
                        }

                        i += 1;
                    }
                }

                return errors.Select(errors => errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success);
            }

            return Enumerable.Repeat(IdentityResult.Success, tuples.Count());
        }

        /// <summary>
        /// Returns <see cref="IdentityResult.Success"/> if validation is successful. This is called before saving the user via <see cref="CreateAsync"/> or <see cref="UpdateAsync"/>.
        /// </summary>
        /// <param name="users">The users who should be validated.</param>
        /// <returns>The <see cref="IdentityResult"/>s representing whether validation was successful.</returns>
        protected async Task<IEnumerable<IdentityResult>> ValidateUsersAsync(
            IEnumerable<TUser> users)
        {
            if (SupportsUserSecurityStamp)
            {
                foreach (var stamp in await GetSecurityStore().GetSecurityStampsAsync(users, CancellationToken))
                {
                    if (string.IsNullOrWhiteSpace(stamp))
                    {
                        // TODO: Continue to validate users where stamp is not null then throw an aggregate exception.
                        throw new InvalidOperationException("User security stamp cannot be null.");
                    }
                }
            }

            if (UserValidators.Any())
            {
                var errors = new List<List<IdentityError>>();

                foreach (var validator in UserValidators)
                {
                    var i = 0;

                    foreach (var result in await validator.ValidateAsync(this, users))
                    {
                        if (i < errors.Count)
                        {
                            errors[i].AddRange(result.Errors);
                        }
                        else
                        {
                            errors.Add(result.Errors.ToList());
                        }

                        i += 1;
                    }
                }

                return errors.Select(errors => errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success);
            }

            return Enumerable.Repeat(IdentityResult.Success, users.Count());
        }

        private IBulkUserEmailStore<TUser> GetEmailStore()
        {
            return Store as IBulkUserEmailStore<TUser> ?? throw new NotSupportedException("Store does not implement IBulkUserEmailStore<TUser>.");
        }

        private IBulkUserLoginStore<TUser> GetLoginStore()
        {
            return Store as IBulkUserLoginStore<TUser> ?? throw new NotSupportedException("Store does not implement IBulkUserLoginStore<TUser>.");
        }

        private IBulkUserPasswordStore<TUser> GetPasswordStore()
        {
            return Store as IBulkUserPasswordStore<TUser> ?? throw new NotSupportedException("Store does not implement IBulkUserPasswordStore<TUser>.");
        }

        private IBulkUserSecurityStampStore<TUser> GetSecurityStore()
        {
            return Store as IBulkUserSecurityStampStore<TUser> ?? throw new NotSupportedException("Store does not implement IBulkUserSecurityStampStore<TUser>.");
        }

        private IBulkUserLockoutStore<TUser> GetUserLockoutStore()
        {
            return Store as IBulkUserLockoutStore<TUser> ?? throw new NotSupportedException("Store does not implement IBulkUserLockoutStore<TUser>.");
        }

        private IBulkUserRoleStore<TUser> GetUserRoleStore()
        {
            return Store as IBulkUserRoleStore<TUser> ?? throw new NotSupportedException("Store does not implement IBulkUserRoleStore<TUser>.");
        }

        private IEnumerable<string?> ProtectPersonalData(
            IEnumerable<string?> data)
        {
            if (Options.Stores.ProtectPersonalData)
            {
                var keyRing = _services!.GetRequiredService<ILookupProtectorKeyRing>();
                var protector = _services!.GetRequiredService<ILookupProtector>();

                return data.Select(data => protector.Protect(keyRing.CurrentKeyId, data));
            }

            return data;
        }

        private async Task UpdateSecurityStampsInternal(
            IEnumerable<TUser> users)
        {
            if (SupportsUserSecurityStamp)
            {
                var tuples = new List<(TUser, string?)>();
            
                foreach (var user in users)
                {
                    tuples.Add((user, Base32.GenerateBase32()));
                }

                await GetSecurityStore().SetSecurityStampsAsync(tuples, CancellationToken);
            }
        }
    }
}
