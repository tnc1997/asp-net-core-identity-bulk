using System.Collections.Generic;
using NUnit.Framework;

namespace Tnc1997.AspNetCore.Identity.Bulk.Tests
{
    public class UpperInvariantBulkLookupNormalizerTests
    {
        [TestCaseSource(nameof(NormalizeEmailsTestCases))]
        public void NormalizeEmails(IEnumerable<string?> emails, IEnumerable<string?> expected)
        {
            // Arrange
            var normalizer = new UpperInvariantBulkLookupNormalizer();

            // Act
            var actual = normalizer.NormalizeEmails(emails);

            // Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        public static IEnumerable<TestCaseData> NormalizeEmailsTestCases
        {
            get
            {
                yield return new TestCaseData(
                    new[] { "alice@example.com", "bob@example.com" },
                    new[] { "ALICE@EXAMPLE.COM", "BOB@EXAMPLE.COM" }
                ).SetName("NormalizeEmails_Email_NormalizedEmail");

                yield return new TestCaseData(
                    new string?[] { null, null },
                    new string?[] { null, null }
                ).SetName("NormalizeEmails_Null_Null");
            }
        }

        [TestCaseSource(nameof(NormalizeNamesTestCases))]
        public void NormalizeNames(IEnumerable<string?> names, IEnumerable<string?> expected)
        {
            // Arrange
            var normalizer = new UpperInvariantBulkLookupNormalizer();

            // Act
            var actual = normalizer.NormalizeNames(names);

            // Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        public static IEnumerable<TestCaseData> NormalizeNamesTestCases
        {
            get
            {
                yield return new TestCaseData(
                    new[] { "alice", "bob" },
                    new[] { "ALICE", "BOB" }
                ).SetName("NormalizeNames_Name_NormalizedName");

                yield return new TestCaseData(
                    new string?[] { null, null },
                    new string?[] { null, null }
                ).SetName("NormalizeNames_Null_Null");
            }
        }
    }
}
