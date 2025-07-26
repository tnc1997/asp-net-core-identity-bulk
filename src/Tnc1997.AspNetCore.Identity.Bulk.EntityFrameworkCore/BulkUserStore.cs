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
/// <typeparam name="TKey">The type of the primary key for a user.</typeparam>
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
/// <typeparam name="TKey">The type of the primary key for a user.</typeparam>
/// <typeparam name="TUserClaim">The type representing a claim.</typeparam>
/// <typeparam name="TUserRole">The type representing a user role.</typeparam>
/// <typeparam name="TUserLogin">The type representing a user external login.</typeparam>
/// <typeparam name="TUserToken">The type representing a user token.</typeparam>
/// <typeparam name="TRoleClaim">The type representing a role claim.</typeparam>
public class BulkUserStore<TUser, TRole, TContext, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>(
    TContext context,
    IdentityErrorDescriber? describer = null)
    : IBulkUserEmailStore<TUser>, IBulkUserLockoutStore<TUser>, IBulkUserLoginStore<TUser>, IBulkUserRoleStore<TUser>, IBulkUserSecurityStampStore<TUser>
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
        IEnumerable<(TUser, string?)> tuples,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ObjectDisposedException.ThrowIf(_disposed, this);
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
        ObjectDisposedException.ThrowIf(_disposed, this);
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
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(users);

        return Task.FromResult(users.Select(user => user.LockoutEnabled));
    }

    public virtual Task SetLockoutEnabledAsync(
        IEnumerable<(TUser, bool)> tuples,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(tuples);

        foreach (var (user, lockoutEnabled) in tuples)
        {
            user.LockoutEnabled = lockoutEnabled;
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
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(tuples);

        var entities = tuples.Select(tuple => new TUserLogin
        {
            UserId = tuple.Item1.Id,
            LoginProvider = tuple.Item2.LoginProvider,
            ProviderKey = tuple.Item2.ProviderKey,
            ProviderDisplayName = tuple.Item2.ProviderDisplayName
        });

        context.AddRange(entities);

        return Task.CompletedTask;
    }

    public virtual async Task<IEnumerable<TUser?>> FindByLoginsAsync(
        IEnumerable<(string, string)> tuples,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(tuples);

        var loginProviders = new List<string>();
        var providerKeys = new List<string>();

        foreach (var (loginProvider, providerKey) in tuples)
        {
            loginProviders.Add(loginProvider);
            providerKeys.Add(providerKey);
        }

        var args = await context
            .Set<TUserLogin>()
            .Where(userLogin => loginProviders.Contains(userLogin.LoginProvider) && providerKeys.Contains(userLogin.ProviderKey))
            .GroupJoin(context.Set<TUser>(), userLogin => userLogin.UserId, user => user.Id, (userLogin, users) => new { UserLogin = userLogin, Users = users })
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

    public virtual async Task RemoveLoginsAsync(
        IEnumerable<(TUser, string, string)> tuples,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ObjectDisposedException.ThrowIf(_disposed, this);
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

        var userLogins = await context
            .Set<TUserLogin>()
            .Where(userLogin => userIds.Contains(userLogin.UserId) && loginProviders.Contains(userLogin.LoginProvider) && providerKeys.Contains(userLogin.ProviderKey))
            .ToListAsync(cancellationToken);

        context.RemoveRange(userLogins);
    }

    #endregion

    #region IBulkUserRoleStore

    public virtual async Task AddToRolesAsync(
        IEnumerable<(TUser, string)> tuples,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(tuples);

        var userIds = new List<TKey>();
        var normalizedNames = new List<string?>();

        foreach (var (user, normalizedName) in tuples)
        {
            userIds.Add(user.Id);
            normalizedNames.Add(normalizedName);
        }

        var roles = await context
            .Set<TRole>()
            .Where(role => normalizedNames.Contains(role.NormalizedName))
            .ToListAsync(cancellationToken);

        var exceptions = new List<Exception>();

        for (var i = 0; i < normalizedNames.Count; i++)
        {
            if (roles.SingleOrDefault(role => role.NormalizedName == normalizedNames[i]) is {  } role)
            {
                context.Add(new TUserRole { UserId = userIds[i], RoleId = role.Id });
            }
            else
            {
                exceptions.Add(new InvalidOperationException($"Role {normalizedNames[i]} does not exist."));
            }
        }

        if (exceptions.Count > 0)
        {
            throw new AggregateException(exceptions);
        }
    }

    public virtual async Task<IEnumerable<bool>> AreInRolesAsync(
        IEnumerable<(TUser, string)> tuples,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(tuples);

        var userIds = new List<TKey>();
        var normalizedNames = new List<string?>();

        foreach (var (user, normalizedName) in tuples)
        {
            userIds.Add(user.Id);
            normalizedNames.Add(normalizedName);
        }

        var userRoleUserIds = await context
            .Set<TRole>()
            .Where(role => normalizedNames.Contains(role.NormalizedName))
            .Join(context.Set<TUserRole>(), role => role.Id, userRole => userRole.RoleId, (role, userRole) => userRole.UserId)
            .ToListAsync(cancellationToken);

        return userIds
            .Select(userId => userRoleUserIds.Contains(userId))
            .ToList();
    }

    public virtual async Task RemoveFromRolesAsync(
        IEnumerable<(TUser, string)> tuples,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(tuples);

        var userIds = new List<TKey>();
        var normalizedNames = new List<string?>();

        foreach (var (user, normalizedName) in tuples)
        {
            userIds.Add(user.Id);
            normalizedNames.Add(normalizedName);
        }

        var userRoles = await context
            .Set<TRole>()
            .Where(role => normalizedNames.Contains(role.NormalizedName))
            .Join(context.Set<TUserRole>(), role => role.Id, userRole => userRole.RoleId, (role, userRole) => userRole)
            .Where(userRole => userIds.Contains(userRole.UserId))
            .ToListAsync(cancellationToken);
        
        context.RemoveRange(userRoles);
    }

    #endregion

    #region IBulkUserSecurityStampStore

    public Task<IEnumerable<string?>> GetSecurityStampsAsync(
	    IEnumerable<TUser> users,
	    CancellationToken cancellationToken)
    {
	    cancellationToken.ThrowIfCancellationRequested();
	    ObjectDisposedException.ThrowIf(_disposed, this);
	    ArgumentNullException.ThrowIfNull(users);

	    return Task.FromResult(users.Select(user => user.SecurityStamp));
    }

    public Task SetSecurityStampsAsync(
	    IEnumerable<(TUser, string?)> tuples,
	    CancellationToken cancellationToken)
    {
	    cancellationToken.ThrowIfCancellationRequested();
	    ObjectDisposedException.ThrowIf(_disposed, this);
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
        IEnumerable<(TUser, string?)> tuples,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ObjectDisposedException.ThrowIf(_disposed, this);
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
        ObjectDisposedException.ThrowIf(_disposed, this);
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
