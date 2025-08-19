using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace Tnc1997.AspNetCore.Identity.Bulk.Tests
{
    public class IdentityResultComparer : IEqualityComparer<IdentityResult>
    {
        private readonly IdentityErrorComparer _comparer = new();

        public bool Equals(IdentityResult? x, IdentityResult? y)
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

            return Enumerable.SequenceEqual(x.Errors, y.Errors, _comparer);
        }

        public int GetHashCode(IdentityResult obj)
        {
            return obj.Errors.GetHashCode();
        }
    }
}
