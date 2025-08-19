using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tnc1997.AspNetCore.Identity.Bulk;
using Tnc1997.AspNetCore.Identity.Bulk.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains extension methods to <see cref="IdentityBuilder"/> for adding bulk Entity Framework stores.
    /// </summary>
    public static class BulkEntityFrameworkIdentityBuilderExtensions
    {
        /// <summary>
        /// Adds an Entity Framework implementation of bulk identity information stores.
        /// </summary>
        /// <typeparam name="TContext">The Entity Framework database context to use.</typeparam>
        /// <param name="builder">The current <see cref="IdentityBuilder"/> instance.</param>
        /// <returns>The current <see cref="IdentityBuilder"/> instance.</returns>
        public static IdentityBuilder AddBulkEntityFrameworkStores<TContext>(
            this IdentityBuilder builder)
            where TContext : DbContext
        {
            var contextType = typeof(TContext);

            var userType = builder.UserType;
            var roleType = builder.RoleType;

            if (FindGenericBaseType(userType, typeof(IdentityUser<>)) is not { } identityUserType)
            {
                throw new InvalidOperationException("AddBulkEntityFrameworkStores can only be called with a user that derives from IdentityUser<TKey>.");
            }

            var keyType = identityUserType.GenericTypeArguments[0];

            if (roleType is not null)
            {
                if (FindGenericBaseType(roleType, typeof(IdentityRole<>)) is not { } identityRoleType)
                {
                    throw new InvalidOperationException("AddBulkEntityFrameworkStores can only be called with a role that derives from IdentityRole<TKey>.");
                }

                Type userStoreType;

                if (FindGenericBaseType(contextType, typeof(IdentityDbContext<,,,,,,,>)) is not { } identityContext)
                {
                    userStoreType = typeof(BulkUserStore<,,,>).MakeGenericType(
                        userType,
                        roleType,
                        contextType,
                        keyType);
                }
                else
                {
                    userStoreType = typeof(BulkUserStore<,,,,,,,,>).MakeGenericType(
                        userType,
                        roleType,
                        contextType,
                        identityContext.GenericTypeArguments[2],
                        identityContext.GenericTypeArguments[3],
                        identityContext.GenericTypeArguments[4],
                        identityContext.GenericTypeArguments[5],
                        identityContext.GenericTypeArguments[7],
                        identityContext.GenericTypeArguments[6]);
                }

                builder.Services.AddScoped(
                    typeof(IBulkUserStore<>).MakeGenericType(
                        userType),
                    userStoreType);
            }
            else
            {
                Type userStoreType;

                if (FindGenericBaseType(contextType, typeof(IdentityUserContext<,,,,>)) is not { } identityContext)
                {
                    userStoreType = typeof(BulkUserOnlyStore<,,>).MakeGenericType(
                        userType,
                        contextType,
                        keyType);
                }
                else
                {
                    userStoreType = typeof(BulkUserOnlyStore<,,,,,>).MakeGenericType(
                        userType,
                        contextType,
                        identityContext.GenericTypeArguments[1],
                        identityContext.GenericTypeArguments[2],
                        identityContext.GenericTypeArguments[3],
                        identityContext.GenericTypeArguments[4]);
                }

                builder.Services.AddScoped(
                    typeof(IBulkUserStore<>).MakeGenericType(
                        userType),
                    userStoreType);
            }

            return builder;
        }

        private static Type? FindGenericBaseType(
            Type currentType,
            Type genericBaseType)
        {
            Type? type = currentType;

            while (type is not null)
            {
                var genericType = type.IsGenericType ? type.GetGenericTypeDefinition() : null;

                if (genericType is not null && genericType == genericBaseType)
                {
                    return type;
                }

                type = type.BaseType;
            }

            return null;
        }
    }
}
