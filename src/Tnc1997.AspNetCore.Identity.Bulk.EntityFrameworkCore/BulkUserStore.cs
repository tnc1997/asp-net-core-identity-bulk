using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Tnc1997.AspNetCore.Identity.Bulk.EntityFrameworkCore;

/// <summary>
/// Represents a new instance of a persistence store for users, using the default implementation
/// of <see cref="IdentityUser{TKey}"/> with a string as a primary key.
/// </summary>
public class BulkUserStore(
    DbContext context,
    IdentityErrorDescriber? describer = null)
    : BulkUserStore<IdentityUser<string>>(context, describer);

/// <summary>
/// Creates a new instance of a persistence store for the specified user type.
/// </summary>
/// <typeparam name="TUser">The type representing a user.</typeparam>
public class BulkUserStore<TUser>(
    DbContext context,
    IdentityErrorDescriber? describer = null)
    : BulkUserStore<TUser, IdentityRole, DbContext, string>(context, describer)
    where TUser : IdentityUser<string>;

/// <summary>
/// Represents a new instance of a persistence store for the specified user and role types.
/// </summary>
/// <typeparam name="TUser">The type representing a user.</typeparam>
/// <typeparam name="TRole">The type representing a role.</typeparam>
/// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
public class BulkUserStore<TUser, TRole, TContext>(
    TContext context,
    IdentityErrorDescriber? describer = null)
    : BulkUserStore<TUser, TRole, TContext, string>(context, describer)
    where TUser : IdentityUser<string>
    where TRole : IdentityRole<string>
    where TContext : DbContext;

/// <summary>
/// Represents a new instance of a persistence store for the specified user and role types.
/// </summary>
/// <typeparam name="TUser">The type representing a user.</typeparam>
/// <typeparam name="TRole">The type representing a role.</typeparam>
/// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
/// <typeparam name="TKey">The type of the primary key for a role.</typeparam>
public class BulkUserStore<TUser, TRole, TContext, TKey>(
    TContext context,
    IdentityErrorDescriber? describer = null)
    : BulkUserStore<TUser, TRole, TContext, TKey, IdentityUserClaim<TKey>, IdentityUserRole<TKey>, IdentityUserLogin<TKey>, IdentityUserToken<TKey>, IdentityRoleClaim<TKey>>(context, describer)
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TContext : DbContext
    where TKey : IEquatable<TKey>;

/// <summary>
/// Represents a new instance of a persistence store for the specified user and role types.
/// </summary>
/// <typeparam name="TUser">The type representing a user.</typeparam>
/// <typeparam name="TRole">The type representing a role.</typeparam>
/// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
/// <typeparam name="TKey">The type of the primary key for a role.</typeparam>
/// <typeparam name="TUserClaim">The type representing a claim.</typeparam>
/// <typeparam name="TUserRole">The type representing a user role.</typeparam>
/// <typeparam name="TUserLogin">The type representing a user external login.</typeparam>
/// <typeparam name="TUserToken">The type representing a user token.</typeparam>
/// <typeparam name="TRoleClaim">The type representing a role claim.</typeparam>
public class BulkUserStore<TUser, TRole, TContext, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>(
    TContext context,
    IdentityErrorDescriber? describer = null)
    : IBulkUserEmailStore<TUser>, IBulkUserLockoutStore<TUser>, IBulkUserLoginStore<TUser>
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TContext : DbContext
    where TKey : IEquatable<TKey>
    where TUserClaim : IdentityUserClaim<TKey>, new()
    where TUserRole : IdentityUserRole<TKey>, new()
    where TUserLogin : IdentityUserLogin<TKey>, new()
    where TUserToken : IdentityUserToken<TKey>, new()
    where TRoleClaim : IdentityRoleClaim<TKey>, new()
{
    private readonly IdentityErrorDescriber _describer = describer ?? new IdentityErrorDescriber();

    private bool _disposed;

    #region IBulkUserEmailStore

    public virtual Task<IEnumerable<string?>> GetEmailsAsync(
        IEnumerable<TUser> users,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(users);

        return Task.FromResult(users.Select(user => user.Email));
    }

    public virtual Task<IEnumerable<string?>> GetNormalizedEmailsAsync(
        IEnumerable<TUser> users,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(users);

        return Task.FromResult(users.Select(user => user.NormalizedEmail));
    }

    public virtual Task SetEmailsAsync(
        IEnumerable<TUser> users,
        IEnumerable<string?> emails,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(users);
        ArgumentNullException.ThrowIfNull(emails);

        foreach (var (user, email) in users.Zip(emails))
        {
            user.Email = email;
        }

        return Task.CompletedTask;
    }

    public virtual Task SetNormalizedEmailsAsync(
        IEnumerable<TUser> users,
        IEnumerable<string?> normalizedEmails,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(users);
        ArgumentNullException.ThrowIfNull(normalizedEmails);

        foreach (var (user, normalizedEmail) in users.Zip(normalizedEmails))
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
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(users);

        return Task.FromResult(users.Select(user => user.LockoutEnabled));
    }

    public virtual Task SetLockoutEnabledAsync(
        IEnumerable<TUser> users,
        IEnumerable<bool> enabled,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(users);
        ArgumentNullException.ThrowIfNull(enabled);

        foreach (var (user, lockoutEnabled) in users.Zip(enabled))
        {
            user.LockoutEnabled = lockoutEnabled;
        }

        return Task.CompletedTask;
    }

    #endregion

    #region IBulkUserLoginStore

    public virtual Task AddLoginsAsync(
        IEnumerable<TUser> users,
        IEnumerable<UserLoginInfo> logins,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(users);
        ArgumentNullException.ThrowIfNull(logins);

        foreach (var (user, login) in users.Zip(logins))
        {
            context.Add(new TUserLogin
            {
                UserId = user.Id,
                LoginProvider = login.LoginProvider,
                ProviderKey = login.ProviderKey,
                ProviderDisplayName = login.ProviderDisplayName
            });
        }

        return Task.CompletedTask;
    }

    public virtual async Task RemoveLoginsAsync(
        IEnumerable<TUser> users,
        IEnumerable<string> loginProviders,
        IEnumerable<string> providerKeys,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(users);
        ArgumentNullException.ThrowIfNull(loginProviders);
        ArgumentNullException.ThrowIfNull(providerKeys);

        var userLogins = users
            .Zip(loginProviders, providerKeys)
            .Select(tuple => new TUserLogin { UserId = tuple.First.Id, LoginProvider = tuple.Second, ProviderKey = tuple.Third })
            .ToList();

        var entities = await context
            .Set<TUserLogin>()
            .Where(entity => userLogins.Any(userLogin => entity.UserId.Equals(userLogin.UserId) && entity.LoginProvider == userLogin.LoginProvider && entity.ProviderKey == userLogin.ProviderKey))
            .ToListAsync(cancellationToken);

        context.RemoveRange(entities);
    }

    #endregion

    #region IBulkUserStore

    public virtual async Task<IEnumerable<IdentityResult>> CreateAsync(
        IEnumerable<TUser> users,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(users);

        context.AddRange(users);

        await context.SaveChangesAsync(cancellationToken);

        return Enumerable.Repeat(IdentityResult.Success, users.Count());
    }

    public virtual async Task<IEnumerable<IdentityResult>> DeleteAsync(
        IEnumerable<TUser> users,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(users);

        context.RemoveRange(users);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Enumerable.Repeat(IdentityResult.Failed(_describer.ConcurrencyFailure()), users.Count());
        }

        return Enumerable.Repeat(IdentityResult.Success, users.Count());
    }

    public virtual Task<IEnumerable<string?>> GetNormalizedUserNamesAsync(
        IEnumerable<TUser> users,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(users);

        return Task.FromResult(users.Select(user => user.NormalizedUserName));
    }

    public virtual Task<IEnumerable<string?>> GetUserNamesAsync(
        IEnumerable<TUser> users,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(users);

        return Task.FromResult(users.Select(user => user.UserName));
    }

    public virtual Task SetNormalizedUserNamesAsync(
        IEnumerable<TUser> users,
        IEnumerable<string?> normalizedNames,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(users);
        ArgumentNullException.ThrowIfNull(normalizedNames);

        foreach (var (user, normalizedName) in users.Zip(normalizedNames))
        {
            user.NormalizedUserName = normalizedName;
        }

        return Task.CompletedTask;
    }

    public virtual Task SetUserNamesAsync(
        IEnumerable<TUser> users,
        IEnumerable<string?> userNames,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(users);
        ArgumentNullException.ThrowIfNull(userNames);

        foreach (var (user, userName) in users.Zip(userNames))
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
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(users);

        foreach (var user in users)
        {
            context.Attach(user);
            user.ConcurrencyStamp = Guid.NewGuid().ToString();
            context.Update(user);
        }

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Enumerable.Repeat(IdentityResult.Failed(_describer.ConcurrencyFailure()), users.Count());
        }

        return Enumerable.Repeat(IdentityResult.Success, users.Count());
    }

    #endregion

    public void Dispose()
    {
        _disposed = true;

        GC.SuppressFinalize(this);
    }
}
