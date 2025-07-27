using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Tnc1997.AspNetCore.Identity.Bulk.EntityFrameworkCore.Tests;

public class IdentityUserRoleComparer : IComparer<IdentityUserRole<string>>
{
    public int Compare(IdentityUserRole<string>? x, IdentityUserRole<string>? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (y is null)
        {
            return 1;
        }

        if (x is null)
        {
            return -1;
        }

        if (string.Compare(x.UserId, y.UserId, StringComparison.Ordinal) is var userIdComparison and not 0)
        {
            return userIdComparison;
        }

        if (string.Compare(x.RoleId, y.RoleId, StringComparison.Ordinal) is var roleIdComparison and not 0)
        {
            return roleIdComparison;
        }

        return 0;
    }
}
