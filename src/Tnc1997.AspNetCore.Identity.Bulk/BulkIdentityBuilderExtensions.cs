using Microsoft.AspNetCore.Identity;
using Tnc1997.AspNetCore.Identity.Bulk;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Helper functions for adding bulk identity services.
/// </summary>
public static class BulkIdentityBuilderExtensions
{
    /// <summary>
    /// Adds the bulk identity services.
    /// </summary>
    /// <param name="builder">The current <see cref="IdentityBuilder"/> instance.</param>
    /// <returns>The current <see cref="IdentityBuilder"/> instance.</returns>
    public static IdentityBuilder AddBulk(
        this IdentityBuilder builder)
    {
        builder.Services.AddScoped(
            typeof(IBulkUserValidator<>).MakeGenericType(
                builder.UserType),
            typeof(BulkUserValidator<>).MakeGenericType(
                builder.UserType));

        builder.Services.AddScoped(
            typeof(IBulkPasswordValidator<>).MakeGenericType(
                builder.UserType),
            typeof(BulkPasswordValidator<>).MakeGenericType(
                builder.UserType));

        builder.Services.AddScoped(
            typeof(IBulkPasswordHasher<>).MakeGenericType(
                builder.UserType),
            typeof(BulkPasswordHasher<>).MakeGenericType(
                builder.UserType));

        builder.Services.AddScoped(
            typeof(IBulkLookupNormalizer),
            typeof(UpperInvariantBulkLookupNormalizer));

        builder.Services.AddScoped(
            typeof(BulkUserManager<>).MakeGenericType(
                builder.UserType));

        return builder;
    }
}
