using System.Collections.Generic;
using System.Linq;

namespace Tnc1997.AspNetCore.Identity.Bulk;

public class UpperInvariantBulkLookupNormalizer : IBulkLookupNormalizer
{
    public IEnumerable<string?> NormalizeNames(IEnumerable<string?> names)
    {
        return names.Select(name => name?.Normalize().ToUpperInvariant());
    }

    public IEnumerable<string?> NormalizeEmails(IEnumerable<string?> emails)
    {
        return emails.Select(email => email?.Normalize().ToUpperInvariant());
    }
}
