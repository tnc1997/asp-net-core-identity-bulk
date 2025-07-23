using System.Collections.Generic;

namespace Tnc1997.AspNetCore.Identity.Bulk;

public interface IBulkLookupNormalizer
{
    /// <summary>
    /// Returns the normalized representations of the specified <paramref name="emails"/>.
    /// </summary>
    /// <param name="emails">The emails to normalize.</param>
    /// <returns>The normalized representations of the specified <paramref name="emails"/>.</returns>
    IEnumerable<string?> NormalizeEmails(IEnumerable<string?> emails);

    /// <summary>
    /// Returns the normalized representations of the specified <paramref name="names"/>.
    /// </summary>
    /// <param name="names">The names to normalize.</param>
    /// <returns>The normalized representations of the specified <paramref name="names"/>.</returns>
    IEnumerable<string?> NormalizeNames(IEnumerable<string?> names);
}
