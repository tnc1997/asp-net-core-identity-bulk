using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Tnc1997.AspNetCore.Identity.Bulk.EntityFrameworkCore.Tests;

public class IdentityUserComparer : IComparer<IdentityUser>
{
    public int Compare(IdentityUser? x, IdentityUser? y)
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

        return string.Compare(x.UserName, y.UserName, StringComparison.Ordinal);
    }
}
