using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Tnc1997.AspNetCore.Identity.Bulk.EntityFrameworkCore.Tests;

public class UserLoginInfoComparer : IComparer<UserLoginInfo>
{
    public int Compare(UserLoginInfo? x, UserLoginInfo? y)
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

        if (string.Compare(x.LoginProvider, y.LoginProvider, StringComparison.Ordinal) is var loginProviderComparison and not 0)
        {
            return loginProviderComparison;
        }

        if (string.Compare(x.ProviderKey, y.ProviderKey, StringComparison.Ordinal) is var providerKeyComparison and not 0)
        {
            return providerKeyComparison;
        }

        if (string.Compare(x.ProviderDisplayName, y.ProviderDisplayName, StringComparison.Ordinal) is var providerDisplayNameComparison and not 0)
        {
            return providerDisplayNameComparison;
        }

        return 0;
    }
}
