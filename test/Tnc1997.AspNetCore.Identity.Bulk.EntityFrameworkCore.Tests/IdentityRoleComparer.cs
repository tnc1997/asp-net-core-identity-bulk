using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Tnc1997.AspNetCore.Identity.Bulk.EntityFrameworkCore.Tests
{
    public class IdentityRoleComparer : IComparer<IdentityRole<string>>
    {
        public int Compare(IdentityRole<string>? x, IdentityRole<string>? y)
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

            return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
        }
    }
}
