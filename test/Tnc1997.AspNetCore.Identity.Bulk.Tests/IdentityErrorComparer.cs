using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Tnc1997.AspNetCore.Identity.Bulk.Tests
{
    public class IdentityErrorComparer : IEqualityComparer<IdentityError>
    {
        public bool Equals(IdentityError? x, IdentityError? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null)
            {
                return false;
            }

            if (y is null)
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            return x.Code == y.Code;
        }

        public int GetHashCode(IdentityError obj)
        {
            return obj.Code.GetHashCode();
        }
    }
}
