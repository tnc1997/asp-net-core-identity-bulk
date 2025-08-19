using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;

namespace Tnc1997.AspNetCore.Identity.Bulk.Tests
{
    public class BulkPasswordValidatorTests
    {
        private BulkUserManager<IdentityUser<string>> _manager;

        [SetUp]
        public void SetUp()
        {
            _manager = Substitute.For<BulkUserManager<IdentityUser<string>>>(Substitute.For<IBulkUserStore<IdentityUser<string>>>(), Substitute.For<IOptions<IdentityOptions>>(), null, null, null, null, null, null, null);
        }

        [TestCaseSource(nameof(ValidateAsyncTestCases))]
        public async Task ValidateAsync(IEnumerable<string?> passwords, IEnumerable<IdentityResult> expected)
        {
            // Arrange
            var validator = new BulkPasswordValidator<IdentityUser<string>>();

            _manager
                .Options
                .Returns(new IdentityOptions());

            var users = passwords.Select(password => (new IdentityUser<string>(), password)).ToList();

            // Act
            var actual = await validator.ValidateAsync(_manager, users);

            // Assert
            Assert.That(actual, Is.EqualTo(expected).Using(new IdentityResultComparer()));
        }

        public static IEnumerable<TestCaseData> ValidateAsyncTestCases
        {
            get
            {
                yield return new TestCaseData(
                    new[] { "" },
                    new[]
                    {
                        IdentityResult.Failed(
                            new IdentityErrorDescriber().PasswordTooShort(6),
                            new IdentityErrorDescriber().PasswordRequiresNonAlphanumeric(),
                            new IdentityErrorDescriber().PasswordRequiresDigit(),
                            new IdentityErrorDescriber().PasswordRequiresLower(),
                            new IdentityErrorDescriber().PasswordRequiresUpper(),
                            new IdentityErrorDescriber().PasswordRequiresUniqueChars(1))
                    }
                ).SetName("ValidateAsync_EmptyPassword_Errors");

                yield return new TestCaseData(
                    new[] { "!0Aa" },
                    new[] { IdentityResult.Failed(new IdentityErrorDescriber().PasswordTooShort(6)) }
                ).SetName("ValidateAsync_ShortPassword_PasswordTooShortError");

                yield return new TestCaseData(
                    new[] { "Pa55w0rd" },
                    new[] { IdentityResult.Failed(new IdentityErrorDescriber().PasswordRequiresNonAlphanumeric()) }
                ).SetName("ValidateAsync_PasswordWithoutAlphanumeric_PasswordRequiresNonAlphanumericError");

                yield return new TestCaseData(
                    new[] { "Password!" },
                    new[] { IdentityResult.Failed(new IdentityErrorDescriber().PasswordRequiresDigit()) }
                ).SetName("ValidateAsync_PasswordWithoutDigit_PasswordRequiresDigitError");

                yield return new TestCaseData(
                    new[] { "PA55W0RD!" },
                    new[] { IdentityResult.Failed(new IdentityErrorDescriber().PasswordRequiresLower()) }
                ).SetName("ValidateAsync_PasswordWithoutLower_PasswordRequiresLowerError");

                yield return new TestCaseData(
                    new[] { "pa55w0rd!" },
                    new[] { IdentityResult.Failed(new IdentityErrorDescriber().PasswordRequiresUpper()) }
                ).SetName("ValidateAsync_PasswordWithoutUpper_PasswordRequiresUpperError");

                yield return new TestCaseData(
                    new[] { "Pa55w0rd!" },
                    new[] { IdentityResult.Success }
                ).SetName("ValidateAsync_ValidPassword_SuccessIdentityResult");
            }
        }

        [TearDown]
        public void TearDown()
        {
            _manager.Dispose();
        }
    }
}
