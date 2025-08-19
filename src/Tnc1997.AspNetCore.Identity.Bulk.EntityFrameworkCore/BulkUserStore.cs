using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Tnc1997.AspNetCore.Identity.Bulk.EntityFrameworkCore
{
    /// <summary>
    /// Represents a new instance of a persistence store for users, using the default implementation of <see cref="IdentityUser{TKey}"/> with a string as a primary key.
    /// </summary>
    public class BulkUserStore
        : BulkUserStore<IdentityUser<string>>
    {
        public BulkUserStore(
            DbContext context,
            IdentityErrorDescriber? describer = null)
            : base(
                context,
                describer)
        {
        }
    }

    /// <summary>
    /// Represents a new instance of a persistence store for the specified user type.
    /// </summary>
    /// <typeparam name="TUser">The type representing a user.</typeparam>
    public class BulkUserStore<TUser>
        : BulkUserStore<TUser, IdentityRole<string>, DbContext, string>
        where TUser : IdentityUser<string>
    {
        public BulkUserStore(
            DbContext context,
            IdentityErrorDescriber? describer = null)
            : base(
                context,
                describer)
        {
        }
    }

    /// <summary>
    /// Represents a new instance of a persistence store for the specified user and role types.
    /// </summary>
    /// <typeparam name="TUser">The type representing a user.</typeparam>
    /// <typeparam name="TRole">The type representing a role.</typeparam>
    /// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
    public class BulkUserStore<TUser, TRole, TContext>
        : BulkUserStore<TUser, TRole, TContext, string>
        where TUser : IdentityUser<string>
        where TRole : IdentityRole<string>
        where TContext : DbContext
    {
        public BulkUserStore(
            TContext context,
            IdentityErrorDescriber? describer = null)
            : base(
                context,
                describer)
        {
        }
    }

    /// <summary>
    /// Represents a new instance of a persistence store for the specified user and role types.
    /// </summary>
    /// <typeparam name="TUser">The type representing a user.</typeparam>
    /// <typeparam name="TRole">The type representing a role.</typeparam>
    /// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
    /// <typeparam name="TKey">The type of the primary key for a user.</typeparam>
    public class BulkUserStore<TUser, TRole, TContext, TKey>
        : BulkUserStore<TUser, TRole, TContext, TKey, IdentityUserClaim<TKey>, IdentityUserRole<TKey>, IdentityUserLogin<TKey>, IdentityUserToken<TKey>, IdentityRoleClaim<TKey>>
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public BulkUserStore(
            TContext context,
            IdentityErrorDescriber? describer = null)
            : base(
                context,
                describer)
        {
        }
    }

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
    public class BulkUserStore<TUser, TRole, TContext, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>
        : BulkUserOnlyStore<TUser, TContext, TKey, TUserClaim, TUserLogin, TUserToken>, IBulkUserRoleStore<TUser>
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
        public BulkUserStore(
            TContext context,
            IdentityErrorDescriber? describer = null)
            : base(
                context,
                describer)
        {
        }

        #region IBulkUserRoleStore

        public virtual async Task AddToRolesAsync(
            IEnumerable<(TUser, string)> tuples,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (tuples is null) throw new ArgumentNullException(nameof(tuples));

            var userIds = new List<TKey>();
            var normalizedNames = new List<string?>();

            foreach (var (user, normalizedName) in tuples)
            {
                userIds.Add(user.Id);
                normalizedNames.Add(normalizedName);
            }

            var roles = await Context
                .Set<TRole>()
                .Where(role => normalizedNames.Contains(role.NormalizedName))
                .ToListAsync(cancellationToken);

            var exceptions = new List<Exception>();

            for (var i = 0; i < normalizedNames.Count; i++)
            {
                if (roles.SingleOrDefault(role => role.NormalizedName == normalizedNames[i]) is {  } role)
                {
                    Context.Add(new TUserRole { UserId = userIds[i], RoleId = role.Id });
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
            ThrowIfDisposed();
            if (tuples is null) throw new ArgumentNullException(nameof(tuples));

            var userIds = new List<TKey>();
            var normalizedNames = new List<string?>();

            foreach (var (user, normalizedName) in tuples)
            {
                userIds.Add(user.Id);
                normalizedNames.Add(normalizedName);
            }

            var userRoleUserIds = await Context
                .Set<TRole>()
                .Where(role => normalizedNames.Contains(role.NormalizedName))
                .Join(Context.Set<TUserRole>(), role => role.Id, userRole => userRole.RoleId, (role, userRole) => userRole.UserId)
                .ToListAsync(cancellationToken);

            return userIds
                .Select(userId => userRoleUserIds.Contains(userId))
                .ToList();
        }

        public virtual async Task<IEnumerable<IEnumerable<string>>> GetRolesAsync(
            IEnumerable<TUser> users,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (users is null) throw new ArgumentNullException(nameof(users));

            var userIds = users
                .Select(user => user.Id)
                .ToList();

            var args = await Context
                .Set<TUserRole>()
                .Where(userRole => userIds.Contains(userRole.UserId))
                .Join(Context.Set<TRole>(), userRole => userRole.RoleId, role => role.Id, (userRole, role) => new { userRole.UserId, role.Name })
                .ToListAsync(cancellationToken);

            return userIds
                .Select(userId => args
                    .Where(args => args.UserId.Equals(userId))
                    .Select(args => args.Name)
                    .ToList())
                .ToList();
        }

        public virtual async Task<IEnumerable<IEnumerable<TUser>>> GetUsersInRolesAsync(
            IEnumerable<string> roleNames,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (roleNames is null) throw new ArgumentNullException(nameof(roleNames));

            var args = await Context
                .Set<TRole>()
                .Where(role => roleNames.Contains(role.NormalizedName))
                .Join(Context.Set<TUserRole>(), role => role.Id, userRole => userRole.RoleId, (role, userRole) => new { role.NormalizedName, userRole.UserId })
                .Join(Context.Set<TUser>(), args => args.UserId, user => user.Id, (args, user) => new { args.NormalizedName, User = user })
                .ToListAsync(cancellationToken);
        
            return roleNames
                .Select(roleName => args
                    .Where(args => args.NormalizedName == roleName)
                    .Select(args => args.User)
                    .ToList())
                .ToList();
        }

        public virtual async Task RemoveFromRolesAsync(
            IEnumerable<(TUser, string)> tuples,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (tuples is null) throw new ArgumentNullException(nameof(tuples));

            var userIds = new List<TKey>();
            var normalizedNames = new List<string?>();

            foreach (var (user, normalizedName) in tuples)
            {
                userIds.Add(user.Id);
                normalizedNames.Add(normalizedName);
            }

            var userRoles = await Context
                .Set<TRole>()
                .Where(role => normalizedNames.Contains(role.NormalizedName))
                .Join(Context.Set<TUserRole>(), role => role.Id, userRole => userRole.RoleId, (role, userRole) => userRole)
                .Where(userRole => userIds.Contains(userRole.UserId))
                .ToListAsync(cancellationToken);
        
            Context.RemoveRange(userRoles);
        }

        #endregion
    }
}
