using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Tnc1997.AspNetCore.Identity.Bulk;

public class BulkUserValidator<TUser>(
    IdentityErrorDescriber? errors = null)
    : IBulkUserValidator<TUser>
    where TUser : class
{
    public IdentityErrorDescriber ErrorDescriber { get; } = errors ?? new IdentityErrorDescriber();

    public async Task<IEnumerable<IdentityResult>> ValidateAsync(
        BulkUserManager<TUser> manager,
        IEnumerable<TUser> users)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(users);

        var errors = Enumerable.Repeat(new List<IdentityError>(), users.Count()).ToList();

        await ValidateUserNames(manager, users, errors);

        if (manager.Options.User.RequireUniqueEmail)
        {
            await ValidateEmails(manager, users, errors);
        }

        return errors.Select(errors => errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success);
    }

    private async Task ValidateUserNames(
        BulkUserManager<TUser> manager,
        IEnumerable<TUser> users,
        List<List<IdentityError>> errors)
    {
        var userIds = new List<string>();

        foreach (var userId in await manager.GetUserIdsAsync(users))
        {
            userIds.Add(userId);
        }

        var userNameIndexes = new List<int>();
        var userNames = new List<string>();

        var i = 0;

        foreach (var userName in await manager.GetUserNamesAsync(users))
        {
            if (string.IsNullOrWhiteSpace(userName) || (!string.IsNullOrEmpty(manager.Options.User.AllowedUserNameCharacters) && userName.Any(character => !manager.Options.User.AllowedUserNameCharacters.Contains(character))))
            {
                errors[i].Add(ErrorDescriber.InvalidUserName(userName));
            }
            else
            {
                userNameIndexes.Add(i);
                userNames.Add(userName);
            }

            i += 1;
        }

        var entityIndexes = new List<int>();
        var entities = new List<TUser>();

        i = 0;

        foreach (var entity in await manager.FindByNamesAsync(userNames))
        {
            if (entity is not null)
            {
                entityIndexes.Add(i);
                entities.Add(entity);
            }

            i += 1;
        }

        i = 0;

        foreach (var entity in await manager.GetUserIdsAsync(entities))
        {
            if (!string.Equals(userIds[userNameIndexes[entityIndexes[i]]], entity))
            {
                errors[userNameIndexes[entityIndexes[i]]].Add(ErrorDescriber.DuplicateUserName(userNames.ElementAt(userNameIndexes[entityIndexes[i]])));
            }

            i += 1;
        }
    }

    private async Task ValidateEmails(
        BulkUserManager<TUser> manager,
        IEnumerable<TUser> users,
        List<List<IdentityError>> errors)
    {
        var userIds = new List<string>();

        foreach (var userId in await manager.GetUserIdsAsync(users))
        {
            userIds.Add(userId);
        }

        var emailIndexes = new List<int>();
        var emails = new List<string>();

        var i = 0;

        foreach (var email in await manager.GetEmailsAsync(users))
        {
            if (string.IsNullOrWhiteSpace(email) || new EmailAddressAttribute().IsValid(email) is not true)
            {
                errors[i].Add(ErrorDescriber.InvalidEmail(email));
            }
            else
            {
                emailIndexes.Add(i);
                emails.Add(email);
            }

            i += 1;
        }

        var entityIndexes = new List<int>();
        var entities = new List<TUser>();

        i = 0;

        foreach (var entity in await manager.FindByEmailsAsync(emails))
        {
            if (entity is not null)
            {
                entityIndexes.Add(i);
                entities.Add(entity);
            }

            i += 1;
        }

        i = 0;

        foreach (var entity in await manager.GetUserIdsAsync(entities))
        {
            if (!string.Equals(userIds[emailIndexes[entityIndexes[i]]], entity))
            {
                errors[emailIndexes[entityIndexes[i]]].Add(ErrorDescriber.DuplicateEmail(emails.ElementAt(emailIndexes[entityIndexes[i]])));
            }

            i += 1;
        }
    }
}
