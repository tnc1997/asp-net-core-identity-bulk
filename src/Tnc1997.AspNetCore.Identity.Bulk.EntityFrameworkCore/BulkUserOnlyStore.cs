using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Tnc1997.AspNetCore.Identity.Bulk.EntityFrameworkCore;

/// <summary>
/// Represents a new instance of a persistence store for users, using the default implementation of <see cref="IdentityUser{TKey}"/> with a string as a primary key.
/// </summary>
public class BulkUserOnlyStore(
    DbContext context,
    IdentityErrorDescriber? describer = null)
    : BulkUserOnlyStore<IdentityUser<string>>(
        context,
        describer);

/// <summary>
/// Represents a new instance of a persistence store for the specified user type.
/// </summary>
/// <typeparam name="TUser">The type representing a user.</typeparam>
public class BulkUserOnlyStore<TUser>(
    DbContext context,
    IdentityErrorDescriber? describer = null)
    : BulkUserOnlyStore<TUser, DbContext, string>(
        context,
        describer)
    where TUser : IdentityUser<string>;

/// <summary>
/// Represents a new instance of a persistence store for the specified user type.
/// </summary>
/// <typeparam name="TUser">The type representing a user.</typeparam>
/// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
public class BulkUserOnlyStore<TUser, TContext>(
    TContext context,
    IdentityErrorDescriber? describer = null)
    : BulkUserOnlyStore<TUser, TContext, string>(
        context,
        describer)
    where TUser : IdentityUser<string>
    where TContext : DbContext;

/// <summary>
/// Represents a new instance of a persistence store for the specified user type.
/// </summary>
/// <typeparam name="TUser">The type representing a user.</typeparam>
/// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
/// <typeparam name="TKey">The type of the primary key for a user.</typeparam>
public class BulkUserOnlyStore<TUser, TContext, TKey>(
    TContext context,
    IdentityErrorDescriber? describer = null)
    : BulkUserOnlyStore<TUser, TContext, TKey, IdentityUserClaim<TKey>, IdentityUserLogin<TKey>, IdentityUserToken<TKey>>(
        context,
        describer)
    where TUser : IdentityUser<TKey>
    where TContext : DbContext
    where TKey : IEquatable<TKey>;

/// <summary>
/// Represents a new instance of a persistence store for the specified user type.
/// </summary>
/// <typeparam name="TUser">The type representing a user.</typeparam>
/// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
/// <typeparam name="TKey">The type of the primary key for a user.</typeparam>
/// <typeparam name="TUserClaim">The type representing a claim.</typeparam>
/// <typeparam name="TUserLogin">The type representing a user external login.</typeparam>
/// <typeparam name="TUserToken">The type representing a user token.</typeparam>
public class BulkUserOnlyStore<TUser, TContext, TKey, TUserClaim, TUserLogin, TUserToken>(
    TContext context,
    IdentityErrorDescriber? describer = null)
    : IBulkUserEmailStore<TUser>, IBulkUserLockoutStore<TUser>, IBulkUserLoginStore<TUser>, IBulkUserPasswordStore<TUser>, IBulkUserSecurityStampStore<TUser>
    where TUser : IdentityUser<TKey>
    where TContext : DbContext
    where TKey : IEquatable<TKey>
    where TUserClaim : IdentityUserClaim<TKey>, new()
    where TUserLogin : IdentityUserLogin<TKey>, new()
    where TUserToken : IdentityUserToken<TKey>, new()
{
    private bool _disposed;

    /// <summary>
    /// Gets the database context for this store.
    /// </summary>
    public virtual TContext Context { get; } = context;

    /// <summary>
    /// Gets the <see cref="IdentityErrorDescriber"/> for any error that occurred with the current operation.
    /// </summary>
    public virtual IdentityErrorDescriber ErrorDescriber { get; } = describer ?? new IdentityErrorDescriber();

    #region IBulkUserEmailStore

    public virtual async Task<IEnumerable<TUser?>> FindByEmailsAsync(
        IEnumerable<string> normalizedEmails,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(normalizedEmails);

        var users = await Context
            .Set<TUser>()
            .Where(user => normalizedEmails.Contains(user.NormalizedEmail))
            .ToListAsync(cancellationToken);

        return normalizedEmails.Select(normalizedEmail => users.SingleOrDefault(user => normalizedEmail == user.NormalizedEmail));
    }

    public virtual Task<IEnumerable<bool>> GetEmailConfirmedAsync(
        IEnumerable<TUser> users,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(users);

        return Task.FromResult(users.Select(user => user.EmailConfirmed));
    }

    public virtual Task<IEnumerable<string?>> GetEmailsAsync(
        IEnumerable<TUser> users,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(users);

        return Task.FromResult(users.Select(user => user.Email));
    }

    public virtual Task<IEnumerable<string?>> GetNormalizedEmailsAsync(
        IEnumerable<TUser> users,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(users);

        return Task.FromResult(users.Select(user => user.NormalizedEmail));
    }

    public virtual Task SetEmailConfirmedAsync(
        IEnumerable<(TUser, bool)> tuples,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(tuples);

        foreach (var (user, emailConfirmed) in tuples)
        {
            user.EmailConfirmed = emailConfirmed;
        }

        return Task.CompletedTask;
    }

    public virtual Task SetEmailsAsync(
        IEnumerable<(TUser, string?)> tuples,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(tuples);

        foreach (var (user, email) in tuples)
        {
            user.Email = email;
        }

        return Task.CompletedTask;
    }

    public virtual Task SetNormalizedEmailsAsync(
        IEnumerable<(TUser, string?)> tuples,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(tuples);

        foreach (var (user, normalizedEmail) in tuples)
        {
            user.NormalizedEmail = normalizedEmail;
        }

        return Task.CompletedTask;
    }

    #endregion

    #region IBulkUserLockoutStore

    public virtual Task<IEnumerable<bool>> GetLockoutEnabledAsync(
        IEnumerable<TUser> users,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(users);

        return Task.FromResult(users.Select(user => user.LockoutEnabled));
    }

    public virtual Task<IEnumerable<DateTimeOffset?>> GetLockoutEndDatesAsync(
        IEnumerable<TUser> users,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(users);

        return Task.FromResult(users.Select(user => user.LockoutEnd));
    }

    public virtual Task SetLockoutEnabledAsync(
        IEnumerable<(TUser, bool)> tuples,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(tuples);

        foreach (var (user, lockoutEnabled) in tuples)
        {
            user.LockoutEnabled = lockoutEnabled;
        }

        return Task.CompletedTask;
    }

    public virtual Task SetLockoutEndDatesAsync(
        IEnumerable<(TUser, DateTimeOffset?)> tuples,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(tuples);

        foreach (var (user, lockoutEnd) in tuples)
        {
            user.LockoutEnd = lockoutEnd;
        }

        return Task.CompletedTask;
    }

    #endregion

    #region IBulkUserLoginStore

    public virtual Task AddLoginsAsync(
        IEnumerable<(TUser, UserLoginInfo)> tuples,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(tuples);

        var entities = tuples.Select(tuple => new TUserLogin
        {
            UserId = tuple.Item1.Id,
            LoginProvider = tuple.Item2.LoginProvider,
            ProviderKey = tuple.Item2.ProviderKey,
            ProviderDisplayName = tuple.Item2.ProviderDisplayName
        });

        Context.AddRange(entities);

        return Task.CompletedTask;
    }

    public virtual async Task<IEnumerable<TUser?>> FindByLoginsAsync(
        IEnumerable<(string, string)> tuples,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(tuples);

        var loginProviders = new List<string>();
        var providerKeys = new List<string>();

        foreach (var (loginProvider, providerKey) in tuples)
        {
            loginProviders.Add(loginProvider);
            providerKeys.Add(providerKey);
        }

        var args = await Context
            .Set<TUserLogin>()
            .Where(userLogin => loginProviders.Contains(userLogin.LoginProvider) && providerKeys.Contains(userLogin.ProviderKey))
            .GroupJoin(Context.Set<TUser>(), userLogin => userLogin.UserId, user => user.Id, (userLogin, users) => new { UserLogin = userLogin, Users = users })
            .SelectMany(args => args.Users.DefaultIfEmpty(), (args, user) => new { args.UserLogin, User = user })
            .ToListAsync(cancellationToken);

        var users = new List<TUser?>();

        foreach (var (loginProvider, providerKey) in tuples)
        {
            var user = args
                .Where(args => args.UserLogin.LoginProvider == loginProvider && args.UserLogin.ProviderKey == providerKey)
                .Select(args => args.User)
                .SingleOrDefault();

            users.Add(user);
        }

        return users;
    }

    public virtual async Task<IEnumerable<IEnumerable<UserLoginInfo>>> GetLoginsAsync(
        IEnumerable<TUser> users,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(users);

        var userIds = users
            .Select(user => user.Id)
            .ToList();

        var userLogins = await Context
            .Set<TUserLogin>()
            .Where(userLogin => userIds.Contains(userLogin.UserId))
            .ToListAsync(cancellationToken);

        return userIds
            .Select(userId => userLogins
                .Where(userLogin => userLogin.UserId.Equals(userId))
                .Select(userLogin => new UserLoginInfo(userLogin.LoginProvider, userLogin.ProviderKey, userLogin.ProviderDisplayName))
                .ToList())
            .ToList();
    }

    public virtual async Task RemoveLoginsAsync(
        IEnumerable<(TUser, string, string)> tuples,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(tuples);

        var userIds = new List<TKey>();
        var loginProviders = new List<string>();
        var providerKeys = new List<string>();

        foreach (var (user, loginProvider, providerKey) in tuples)
        {
            userIds.Add(user.Id);
            loginProviders.Add(loginProvider);
            providerKeys.Add(providerKey);
        }

        var userLogins = await Context
            .Set<TUserLogin>()
            .Where(userLogin => userIds.Contains(userLogin.UserId) && loginProviders.Contains(userLogin.LoginProvider) && providerKeys.Contains(userLogin.ProviderKey))
            .ToListAsync(cancellationToken);

        Context.RemoveRange(userLogins);
    }

    #endregion

    #region IBulkUserPasswordStore

    public virtual Task<IEnumerable<string?>> GetPasswordHashesAsync(
        IEnumerable<TUser> users,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(users);

        return Task.FromResult(users.Select(user => user.PasswordHash));
    }

    public virtual Task SetPasswordHashesAsync(IEnumerable<(TUser, string?)> tuples, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(tuples);

        foreach (var (user, passwordHash) in tuples)
        {
            user.PasswordHash = passwordHash;
        }

        return Task.CompletedTask;
    }

    #endregion

    #region IBulkUserSecurityStampStore

    public Task<IEnumerable<string?>> GetSecurityStampsAsync(
	    IEnumerable<TUser> users,
	    CancellationToken cancellationToken)
    {
	    cancellationToken.ThrowIfCancellationRequested();
	    ThrowIfDisposed();
	    ArgumentNullException.ThrowIfNull(users);

	    return Task.FromResult(users.Select(user => user.SecurityStamp));
    }

    public Task SetSecurityStampsAsync(
	    IEnumerable<(TUser, string?)> tuples,
	    CancellationToken cancellationToken)
    {
	    cancellationToken.ThrowIfCancellationRequested();
	    ThrowIfDisposed();
	    ArgumentNullException.ThrowIfNull(tuples);

	    foreach (var (user, securityStamp) in tuples)
	    {
		    user.SecurityStamp = securityStamp;
	    }

	    return Task.CompletedTask;
    }

    #endregion

    #region IBulkUserStore

    public virtual async Task<IEnumerable<IdentityResult>> CreateAsync(
        IEnumerable<TUser> users,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(users);

        Context.AddRange(users);

        await Context.SaveChangesAsync(cancellationToken);

        return Enumerable.Repeat(IdentityResult.Success, users.Count());
    }

    public virtual async Task<IEnumerable<IdentityResult>> DeleteAsync(
        IEnumerable<TUser> users,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(users);

        Context.RemoveRange(users);

        try
        {
            await Context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Enumerable.Repeat(IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure()), users.Count());
        }

        return Enumerable.Repeat(IdentityResult.Success, users.Count());
    }

    public virtual async Task<IEnumerable<TUser?>> FindByIdsAsync(
        IEnumerable<string> userIds,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(userIds);

        var normalizedUserIds = userIds
            .Select(userId => TypeDescriptor.GetConverter(typeof(TKey)).ConvertFromInvariantString(userId))
            .Cast<TKey>()
            .ToList();

        var users = await Context
            .Set<TUser>()
            .Where(user => normalizedUserIds.Contains(user.Id))
            .ToListAsync(cancellationToken);

        return normalizedUserIds.Select(normalizedUserId => users.SingleOrDefault(user => normalizedUserId.Equals(user.Id)));
    }

    public virtual async Task<IEnumerable<TUser?>> FindByNamesAsync(
        IEnumerable<string> normalizedUserNames,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(normalizedUserNames);

        var users = await Context
            .Set<TUser>()
            .Where(user => normalizedUserNames.Contains(user.NormalizedUserName))
            .ToListAsync(cancellationToken);

        return normalizedUserNames.Select(normalizedUserName => users.SingleOrDefault(user => normalizedUserName == user.NormalizedUserName));
    }

    public virtual Task<IEnumerable<string?>> GetNormalizedUserNamesAsync(
        IEnumerable<TUser> users,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(users);

        return Task.FromResult(users.Select(user => user.NormalizedUserName));
    }

    public virtual Task<IEnumerable<string>> GetUserIdsAsync(
        IEnumerable<TUser> users,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(users);

        return Task.FromResult(users.Select(user => user.Id.ToString()!));
    }

    public virtual Task<IEnumerable<string?>> GetUserNamesAsync(
        IEnumerable<TUser> users,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(users);

        return Task.FromResult(users.Select(user => user.UserName));
    }

    public virtual Task SetNormalizedUserNamesAsync(
        IEnumerable<(TUser, string?)> tuples,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(tuples);

        foreach (var (user, normalizedName) in tuples)
        {
            user.NormalizedUserName = normalizedName;
        }

        return Task.CompletedTask;
    }

    public virtual Task SetUserNamesAsync(
        IEnumerable<(TUser, string?)> tuples,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(tuples);

        foreach (var (user, userName) in tuples)
        {
            user.UserName = userName;
        }

        return Task.CompletedTask;
    }

    public virtual async Task<IEnumerable<IdentityResult>> UpdateAsync(
        IEnumerable<TUser> users,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(users);

        foreach (var user in users)
        {
            Context.Attach(user);
            user.ConcurrencyStamp = Guid.NewGuid().ToString();
            Context.Update(user);
        }

        try
        {
            await Context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Enumerable.Repeat(IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure()), users.Count());
        }

        return Enumerable.Repeat(IdentityResult.Success, users.Count());
    }

    #endregion

    public void Dispose()
    {
        _disposed = true;

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Throws if this class has been disposed.
    /// </summary>
    protected void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
